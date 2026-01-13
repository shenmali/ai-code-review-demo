#!/usr/bin/env node

import fs from "fs";
import fetch from "node-fetch";

const diff = fs.readFileSync("diff.txt", "utf8");

if (!diff.trim()) {
  console.log("No diff found, skipping review.");
  process.exit(0);
}

// Commit bilgileri
const commitSha = process.env.COMMIT_SHA;
const commitMessage = process.env.COMMIT_MESSAGE;
const commitAuthor = process.env.COMMIT_AUTHOR;
const repo = process.env.GITHUB_REPOSITORY;

// Gaming-specific prompt
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

DIFF:
${diff}

Commit Message: ${commitMessage}
`;

// OpenRouter API çağrısı
const response = await fetch("https://openrouter.ai/api/v1/chat/completions", {
  method: "POST",
  headers: {
    "Authorization": `Bearer ${process.env.OPENROUTER_API_KEY}`,
    "Content-Type": "application/json",
    "HTTP-Referer": "https://github.com",
    "X-Title": "Game Code Review Bot"
  },
  body: JSON.stringify({
    model: "anthropic/claude-3.5-sonnet",
    messages: [{ role: "user", content: prompt }],
    temperature: 0.2,
    max_tokens: 2000
  })
});

if (!response.ok) {
  console.error(`OpenRouter API error: ${response.status}`);
  process.exit(1);
}

const data = await response.json();
const review = data?.choices?.[0]?.message?.content;

if (!review) {
  console.error("AI review failed: No content in response");
  process.exit(1);
}

// Slack mesajını hazırla
const slackMessage = {
  blocks: [
    {
      type: "header",
      text: {
        type: "plain_text",
        text: "🤖 AI Code Review"
      }
    },
    {
      type: "section",
      fields: [
        {
          type: "mrkdwn",
          text: `*Repo:*\n${repo}`
        },
        {
          type: "mrkdwn",
          text: `*Author:*\n${commitAuthor}`
        },
        {
          type: "mrkdwn",
          text: `*Commit:*\n\`${commitSha}\``
        },
        {
          type: "mrkdwn",
          text: `*Message:*\n${commitMessage}`
        }
      ]
    },
    {
      type: "divider"
    },
    {
      type: "section",
      text: {
        type: "mrkdwn",
        text: review
      }
    },
    {
      type: "actions",
      elements: [
        {
          type: "button",
          text: {
            type: "plain_text",
            text: "View on GitHub"
          },
          url: `https://github.com/${repo}/commit/${commitSha}`
        }
      ]
    }
  ]
};

// Slack'e gönder
const slackResponse = await fetch(process.env.SLACK_WEBHOOK_URL, {
  method: "POST",
  headers: {
    "Content-Type": "application/json"
  },
  body: JSON.stringify(slackMessage)
});

if (!slackResponse.ok) {
  console.error(`Slack webhook error: ${slackResponse.status}`);
  process.exit(1);
}

console.log("✅ AI review sent to Slack successfully!");
