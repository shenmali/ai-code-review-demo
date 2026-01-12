#!/usr/bin/env node

import fs from "fs";
import { execSync } from "child_process";
import fetch from "node-fetch";

// .aiignore dosyasını oku
function loadIgnorePatterns() {
  try {
    const ignoreFile = fs.readFileSync(".aiignore", "utf8");
    return ignoreFile
      .split("\n")
      .map(line => line.trim())
      .filter(line => line && !line.startsWith("#"));
  } catch (error) {
    console.log("No .aiignore file found, using defaults");
    return [];
  }
}

// Diff'i .aiignore'a göre filtrele
function filterDiff(diff, ignorePatterns) {
  if (!ignorePatterns.length) return diff;

  const lines = diff.split("\n");
  const filteredLines = [];
  let currentFile = null;
  let skipCurrentFile = false;

  for (const line of lines) {
    // Yeni dosya başlangıcı
    if (line.startsWith("diff --git")) {
      const match = line.match(/diff --git a\/(.*?) b\/(.*)/);
      currentFile = match ? match[2] : null;
      skipCurrentFile = shouldIgnoreFile(currentFile, ignorePatterns);
    }

    if (!skipCurrentFile) {
      filteredLines.push(line);
    }
  }

  return filteredLines.join("\n");
}

// Dosyanın ignore edilmesi gerekip gerekmediğini kontrol et
function shouldIgnoreFile(filepath, ignorePatterns) {
  if (!filepath) return false;

  for (const pattern of ignorePatterns) {
    // Dizin kontrolü (/ ile biten)
    if (pattern.endsWith("/")) {
      if (filepath.startsWith(pattern) || filepath.includes("/" + pattern)) {
        return true;
      }
    }
    // Wildcard kontrolü
    else if (pattern.includes("*")) {
      const regex = new RegExp("^" + pattern.replace(/\*/g, ".*") + "$");
      if (regex.test(filepath)) {
        return true;
      }
    }
    // Tam eşleşme
    else if (filepath === pattern || filepath.endsWith("/" + pattern)) {
      return true;
    }
  }
  return false;
}

// AI review al
async function getAIReview(diff, commitSha, commitMessage) {
  const prompt = `
Sen Unity 2D casual game development konusunda uzmanlaşmış bir senior software engineer'sın.

Aşağıdaki commit için LINE-BY-LINE kod incelemesi yap:

**Commit SHA:** ${commitSha}
**Commit Message:** ${commitMessage}

**Unity 2D Casual Game Development odaklı inceleme kriterleri:**

🎮 **Gameplay & Performance:**
- MonoBehaviour lifecycle metodlarının doğru kullanımı (Update, FixedUpdate, LateUpdate)
- Gereksiz Update/FixedUpdate çağrıları var mı? (Performance)
- Object pooling kullanılmalı mı?
- Coroutine vs InvokeRepeating kullanımı uygun mu?
- Physics2D ve collision optimizasyonları

🏗️ **Unity Best Practices:**
- Component pattern doğru kullanılmış mı?
- GetComponent çağrıları cache'leniyor mu?
- Singleton pattern abuse var mı?
- ScriptableObject kullanımı uygun mu?
- Serialization ve Inspector kullanımı

⚡ **Mobile Optimization (Casual games için kritik):**
- GC Allocation yaratan kodlar var mı?
- String concatenation yerine StringBuilder kullanılmalı mı?
- LINQ kullanımı performans sorunu yaratır mı?
- Draw call optimizasyonları
- Memory leak riski

🐛 **Bugs & Edge Cases:**
- Null reference hatası riski
- Race condition'lar
- Lifecycle event sıralaması sorunları
- Platform specific sorunlar (iOS/Android)

🎨 **Code Quality:**
- Kod okunabilirliği
- Naming conventions (Unity C# standartları)
- Magic number'lar yerine const/readonly kullanımı
- Region kullanımı ve organizasyon

🔒 **Common Pitfalls:**
- FindObjectOfType her frame'de mi çağrılıyor?
- Animator.SetTrigger yerine SetBool kullanılmalı mı?
- Prefab instantiation optimizasyonları
- Scene yönetimi ve DontDestroyOnLoad kullanımı

**Yanıt formatı:**
- Her satır için ayrı ayrı analiz yap
- Türkçe yaz
- Bullet point kullan
- Sıkı ama yapıcı ol
- Kod örnekleri göster
- Emoji kullan (🔴 kritik, 🟡 uyarı, 🟢 iyi pratik, 💡 öneri)

**DIFF:**
${diff}
`;

  const response = await fetch("https://openrouter.ai/api/v1/chat/completions", {
    method: "POST",
    headers: {
      "Authorization": `Bearer ${process.env.OPENROUTER_API_KEY}`,
      "Content-Type": "application/json",
      "HTTP-Referer": "https://github.com",
      "X-Title": "Unity 2D AI Code Review Bot"
    },
    body: JSON.stringify({
      model: "anthropic/claude-3.5-sonnet",
      messages: [{ role: "user", content: prompt }],
      temperature: 0.2
    })
  });

  if (!response.ok) {
    console.error(`OpenRouter API error: ${response.status} ${response.statusText}`);
    const errorText = await response.text();
    console.error(errorText);
    return null;
  }

  const data = await response.json();
  return data?.choices?.[0]?.message?.content;
}

// PR'a comment ekle
async function postComment(repo, prNumber, comment) {
  const commentResponse = await fetch(
    `https://api.github.com/repos/${repo}/issues/${prNumber}/comments`,
    {
      method: "POST",
      headers: {
        "Authorization": `Bearer ${process.env.GITHUB_TOKEN}`,
        "Content-Type": "application/json",
        "Accept": "application/vnd.github+json"
      },
      body: JSON.stringify({ body: comment })
    }
  );

  if (!commentResponse.ok) {
    console.error(`GitHub API error: ${commentResponse.status} ${commentResponse.statusText}`);
    const errorText = await commentResponse.text();
    console.error(errorText);
    return false;
  }

  return true;
}

// Ana fonksiyon
async function main() {
  // Environment variables
  const eventPath = process.env.GITHUB_EVENT_PATH;
  const repo = process.env.GITHUB_REPOSITORY;
  const baseRef = process.env.PR_BASE_REF;
  const headSha = process.env.PR_HEAD_SHA;

  if (!eventPath) {
    console.error("GITHUB_EVENT_PATH not found.");
    process.exit(1);
  }

  const event = JSON.parse(fs.readFileSync(eventPath, "utf8"));
  const prNumber = event.pull_request?.number;

  if (!prNumber) {
    console.error("PR number not found in event payload.");
    process.exit(1);
  }

  // .aiignore dosyasını yükle
  const ignorePatterns = loadIgnorePatterns();
  console.log(`Loaded ${ignorePatterns.length} ignore patterns`);

  // Base branch'i fetch et
  try {
    execSync(`git fetch origin ${baseRef}`, { stdio: "inherit" });
  } catch (error) {
    console.error("Failed to fetch base branch");
    process.exit(1);
  }

  // PR'daki tüm commitleri al
  let commits;
  try {
    const commitsOutput = execSync(
      `git log origin/${baseRef}..${headSha} --format=%H`,
      { encoding: "utf8" }
    );
    commits = commitsOutput.trim().split("\n").filter(Boolean).reverse();
  } catch (error) {
    console.error("Failed to get commits");
    process.exit(1);
  }

  console.log(`Found ${commits.length} commits to review`);

  if (commits.length === 0) {
    console.log("No commits to review");
    await postComment(repo, prNumber, "✅ Bu PR'da review edilecek commit bulunamadı.");
    process.exit(0);
  }

  // Her commit için review yap
  for (let i = 0; i < commits.length; i++) {
    const commitSha = commits[i];
    const shortSha = commitSha.substring(0, 7);

    console.log(`\n[${i + 1}/${commits.length}] Reviewing commit ${shortSha}...`);

    // Commit mesajını al
    let commitMessage;
    try {
      commitMessage = execSync(`git log -1 --format=%s ${commitSha}`, {
        encoding: "utf8"
      }).trim();
    } catch (error) {
      commitMessage = "Unknown";
    }

    // Commit için diff al
    let diff;
    try {
      const parentSha = i === 0 ? `origin/${baseRef}` : commits[i - 1];
      diff = execSync(`git diff ${parentSha} ${commitSha}`, {
        encoding: "utf8"
      });
    } catch (error) {
      console.error(`Failed to get diff for commit ${shortSha}`);
      continue;
    }

    // Diff'i filtrele
    const filteredDiff = filterDiff(diff, ignorePatterns);

    if (!filteredDiff.trim()) {
      console.log(`No relevant changes in commit ${shortSha} (all files ignored)`);
      continue;
    }

    // AI review al
    const review = await getAIReview(filteredDiff, shortSha, commitMessage);

    if (!review) {
      console.error(`Failed to get review for commit ${shortSha}`);
      continue;
    }

    // Comment formatla ve gönder
    const formattedComment = `## 🎮 Unity 2D Code Review - Commit \`${shortSha}\`

**Commit Message:** ${commitMessage}

---

${review}

---
<sub>🤖 AI-powered review by Claude 3.5 Sonnet</sub>`;

    const success = await postComment(repo, prNumber, formattedComment);

    if (success) {
      console.log(`✅ Review posted for commit ${shortSha}`);
    } else {
      console.error(`❌ Failed to post review for commit ${shortSha}`);
    }

    // Rate limiting için kısa bir bekleme
    if (i < commits.length - 1) {
      await new Promise(resolve => setTimeout(resolve, 2000));
    }
  }

  console.log("\n✅ All reviews completed!");
}

main().catch(error => {
  console.error("Fatal error:", error);
  process.exit(1);
});