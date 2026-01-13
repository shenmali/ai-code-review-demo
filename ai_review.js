#!/usr/bin/env node

import fs from "fs";
import fetch from "node-fetch";
import { minimatch } from "minimatch";

// ============================================================================
// AIIGNORE FILTERING FUNCTIONS
// ============================================================================

/**
 * Load and parse .aiignore file
 * @returns {string[]} Array of ignore patterns
 */
function loadAiignore() {
  try {
    const content = fs.readFileSync('.aiignore', 'utf8');
    return parseAiignore(content);
  } catch (error) {
    if (error.code === 'ENOENT') {
      console.log('ℹ️  No .aiignore file found, reviewing all files');
      return [];
    }
    console.warn('⚠️  Error reading .aiignore:', error.message);
    return [];
  }
}

/**
 * Parse .aiignore content into patterns
 * @param {string} content - File content
 * @returns {string[]} Array of patterns
 */
function parseAiignore(content) {
  return content
    .split('\n')
    .map(line => line.trim())
    .filter(line => line && !line.startsWith('#')); // Remove comments and empty lines
}

/**
 * Split diff into individual file diffs
 * @param {string} diffContent - Full diff content
 * @returns {string[]} Array of individual file diffs
 */
function splitDiffByFile(diffContent) {
  const fileSeparator = /^diff --git /gm;
  const parts = diffContent.split(fileSeparator);

  // First part is empty (before first diff)
  const diffs = parts.slice(1).map(part => 'diff --git ' + part);

  return diffs;
}

/**
 * Extract filename from diff block
 * @param {string} diffBlock - Single file diff
 * @returns {string|null} Filename or null
 */
function extractFilename(diffBlock) {
  // Extract from "+++ b/path/to/file"
  const match = diffBlock.match(/^\+\+\+ b\/(.+)$/m);
  return match ? match[1] : null;
}

/**
 * Check if filename should be ignored
 * @param {string} filename - File path
 * @param {string[]} patterns - Ignore patterns
 * @returns {boolean} True if should be ignored
 */
function shouldIgnore(filename, patterns) {
  if (!filename) return false;

  return patterns.some(pattern => {
    // minimatch handles glob patterns
    return minimatch(filename, pattern, { matchBase: true });
  });
}

/**
 * Filter diff content based on .aiignore patterns
 * @param {string} diffContent - Full diff
 * @param {string[]} patterns - Ignore patterns
 * @returns {string} Filtered diff
 */
function filterDiff(diffContent, patterns) {
  if (!patterns || patterns.length === 0) {
    return diffContent;
  }

  const fileDiffs = splitDiffByFile(diffContent);
  const filtered = fileDiffs.filter(fileDiff => {
    const filename = extractFilename(fileDiff);
    const ignore = shouldIgnore(filename, patterns);

    if (ignore) {
      console.log(`  🚫 Ignoring: ${filename}`);
    }

    return !ignore;
  });

  console.log(`📊 Filtered ${fileDiffs.length - filtered.length} of ${fileDiffs.length} files via .aiignore`);

  return filtered.join('\n\n');
}

// ============================================================================
// REVIEW PARSING FUNCTIONS
// ============================================================================

/**
 * Parse AI review response into categories
 * @param {string} reviewText - AI response
 * @returns {Object} Categorized review
 */
function parseReview(reviewText) {
  const lines = reviewText.split('\n');
  const parsed = {
    critical: [],
    warnings: [],
    suggestions: [],
    good: [],
    other: []
  };

  lines.forEach(line => {
    const trimmed = line.trim();
    if (!trimmed) return;

    if (trimmed.includes('🔴')) {
      parsed.critical.push(trimmed);
    } else if (trimmed.includes('🟡')) {
      parsed.warnings.push(trimmed);
    } else if (trimmed.includes('💡')) {
      parsed.suggestions.push(trimmed);
    } else if (trimmed.includes('🟢')) {
      parsed.good.push(trimmed);
    } else {
      parsed.other.push(trimmed);
    }
  });

  // Fallback: if no emojis found, warn and return all as other
  if (parsed.critical.length === 0 &&
      parsed.warnings.length === 0 &&
      parsed.suggestions.length === 0 &&
      parsed.good.length === 0) {
    console.warn('⚠️  No emoji markers found in AI response, using fallback');
  }

  return parsed;
}

/**
 * Calculate quality score based on issues
 * @param {Object} parsed - Parsed review
 * @returns {number} Score (0-100)
 */
function calculateQualityScore(parsed) {
  let score = 100;

  // Deduct points based on severity
  score -= parsed.critical.length * 20;
  score -= parsed.warnings.length * 10;
  score -= parsed.suggestions.length * 5;

  // Bonus for good practices (up to +10)
  score += Math.min(10, parsed.good.length * 2);

  // Clamp to 0-100
  return Math.max(0, Math.min(100, score));
}

/**
 * Get score emoji indicator
 * @param {number} score - Quality score
 * @returns {string} Emoji with label
 */
function getScoreEmoji(score) {
  if (score >= 90) return '🟢 Excellent';
  if (score >= 75) return '🟡 Good';
  if (score >= 50) return '🟠 Needs Work';
  return '🔴 Critical Issues';
}

/**
 * Generate ASCII progress bar
 * @param {number} score - Quality score
 * @param {number} length - Bar length (default 10)
 * @returns {string} Progress bar string
 */
function generateProgressBar(score, length = 10) {
  const filled = Math.round((score / 100) * length);
  const bar = '█'.repeat(filled) + '░'.repeat(length - filled);
  return `${bar} ${score}/100`;
}

// ============================================================================
// SLACK MESSAGING FUNCTIONS
// ============================================================================

/**
 * Build enhanced main Slack message
 * @param {Object} parsed - Parsed review
 * @param {number} score - Quality score
 * @param {Object} commitInfo - Commit metadata
 * @returns {Object} Slack message blocks
 */
function buildMainMessage(parsed, score, commitInfo) {
  const scoreEmoji = getScoreEmoji(score);
  const progressBar = generateProgressBar(score);

  // Top 3 issues for summary
  const topIssues = [
    ...parsed.critical.slice(0, 1),
    ...parsed.warnings.slice(0, 2),
    ...parsed.suggestions.slice(0, 1)
  ].slice(0, 3);

  const topIssuesSummary = topIssues.length > 0
    ? topIssues.map(issue => `• ${issue.substring(0, 150)}${issue.length > 150 ? '...' : ''}`).join('\n')
    : '• ✅ No significant issues found';

  // Build full review text for webhook approach (no threading)
  const fullReviewSections = [];

  if (parsed.critical.length > 0) {
    fullReviewSections.push(`*🔴 Critical Issues (${parsed.critical.length})*\n${parsed.critical.join('\n')}`);
  }

  if (parsed.warnings.length > 0) {
    fullReviewSections.push(`*🟡 Warnings (${parsed.warnings.length})*\n${parsed.warnings.join('\n')}`);
  }

  if (parsed.suggestions.length > 0) {
    fullReviewSections.push(`*💡 Suggestions (${parsed.suggestions.length})*\n${parsed.suggestions.join('\n')}`);
  }

  if (parsed.good.length > 0) {
    fullReviewSections.push(`*🟢 Good Practices (${parsed.good.length})*\n${parsed.good.join('\n')}`);
  }

  if (parsed.other.length > 0) {
    fullReviewSections.push(`*📝 Other Notes*\n${parsed.other.join('\n')}`);
  }

  const fullReview = fullReviewSections.join('\n\n---\n\n');

  return {
    blocks: [
      {
        type: "header",
        text: {
          type: "plain_text",
          text: `🤖 AI Code Review - ${scoreEmoji}`
        }
      },
      {
        type: "section",
        fields: [
          {
            type: "mrkdwn",
            text: `*Repo:*\n${commitInfo.repo}`
          },
          {
            type: "mrkdwn",
            text: `*Author:*\n${commitInfo.author}`
          },
          {
            type: "mrkdwn",
            text: `*Commit:*\n\`${commitInfo.sha}\``
          },
          {
            type: "mrkdwn",
            text: `*Message:*\n${commitInfo.message}`
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
          text: `*Quality Score:*\n\`${progressBar}\``
        }
      },
      {
        type: "section",
        fields: [
          {
            type: "mrkdwn",
            text: `🔴 *Critical:* ${parsed.critical.length}`
          },
          {
            type: "mrkdwn",
            text: `🟡 *Warnings:* ${parsed.warnings.length}`
          },
          {
            type: "mrkdwn",
            text: `💡 *Suggestions:* ${parsed.suggestions.length}`
          },
          {
            type: "mrkdwn",
            text: `🟢 *Good Practices:* ${parsed.good.length}`
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
          text: `*Top Issues:*\n${topIssuesSummary}`
        }
      },
      {
        type: "divider"
      },
      {
        type: "section",
        text: {
          type: "mrkdwn",
          text: `*📋 Detailed Review:*\n\n${fullReview}`
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
            url: `https://github.com/${commitInfo.repo}/commit/${commitInfo.sha}`
          }
        ]
      }
    ]
  };
}

// ============================================================================
// MAIN EXECUTION
// ============================================================================

async function main() {
  console.log("🚀 Starting AI Code Review...\n");

  // 1. Load diff
  let diff = fs.readFileSync("diff.txt", "utf8");

  if (!diff.trim()) {
    console.log("ℹ️  No diff found, skipping review.");
    process.exit(0);
  }

  // 2. Load and apply .aiignore filtering
  console.log("📁 Loading .aiignore patterns...");
  const aiignorePatterns = loadAiignore();

  if (aiignorePatterns.length > 0) {
    console.log(`🔍 Found ${aiignorePatterns.length} ignore patterns`);
    diff = filterDiff(diff, aiignorePatterns);
  } else {
    console.log("📄 No .aiignore patterns, reviewing all files");
  }

  // 3. Check if anything left to review after filtering
  if (!diff.trim()) {
    console.log("\n✅ No reviewable changes after .aiignore filtering");

    // Send notification to Slack about filtered commit
    const slackMessage = {
      blocks: [
        {
          type: "section",
          text: {
            type: "mrkdwn",
            text: `✅ *No reviewable changes* in commit \`${process.env.COMMIT_SHA}\`\n\n_All files filtered by .aiignore_`
          }
        }
      ]
    };

    await fetch(process.env.SLACK_WEBHOOK_URL, {
      method: "POST",
      headers: { "Content-Type": "application/json" },
      body: JSON.stringify(slackMessage)
    });

    process.exit(0);
  }

  // 4. Get commit metadata
  const commitInfo = {
    sha: process.env.COMMIT_SHA,
    message: process.env.COMMIT_MESSAGE,
    author: process.env.COMMIT_AUTHOR,
    repo: process.env.GITHUB_REPOSITORY
  };

  console.log(`\n📝 Reviewing commit: ${commitInfo.sha}`);
  console.log(`👤 Author: ${commitInfo.author}`);
  console.log(`💬 Message: ${commitInfo.message}\n`);

  // 5. Prepare AI prompt
  const prompt = `
Sen Unity 2D casual game development konusunda uzmanlaşmış bir senior software engineer'sın.

Aşağıdaki commit için LINE-BY-LINE kod incelemesi yap:

**Commit SHA:** ${commitInfo.sha}
**Commit Message:** ${commitInfo.message}

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

Commit Message: ${commitInfo.message}
`;

  // 6. Call OpenRouter API
  console.log("🤖 Calling AI for code review...");
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
    console.error(`❌ OpenRouter API error: ${response.status}`);
    process.exit(1);
  }

  const data = await response.json();
  const review = data?.choices?.[0]?.message?.content;

  if (!review) {
    console.error("❌ AI review failed: No content in response");
    process.exit(1);
  }

  console.log("✅ AI review received\n");

  // 7. Parse review and calculate score
  console.log("📊 Analyzing review results...");
  const parsed = parseReview(review);
  const score = calculateQualityScore(parsed);

  console.log(`\n📈 Quality Score: ${score}/100 (${getScoreEmoji(score)})`);
  console.log(`   🔴 Critical: ${parsed.critical.length}`);
  console.log(`   🟡 Warnings: ${parsed.warnings.length}`);
  console.log(`   💡 Suggestions: ${parsed.suggestions.length}`);
  console.log(`   🟢 Good Practices: ${parsed.good.length}\n`);

  // 8. Build and send enhanced Slack message
  console.log("📤 Sending enhanced review to Slack...");
  const slackMessage = buildMainMessage(parsed, score, commitInfo);

  const slackResponse = await fetch(process.env.SLACK_WEBHOOK_URL, {
    method: "POST",
    headers: {
      "Content-Type": "application/json"
    },
    body: JSON.stringify(slackMessage)
  });

  if (!slackResponse.ok) {
    console.error(`❌ Slack webhook error: ${slackResponse.status}`);
    process.exit(1);
  }

  console.log("✅ AI review sent to Slack successfully!");
  console.log(`🔗 View commit: https://github.com/${commitInfo.repo}/commit/${commitInfo.sha}\n`);
}

// Run main function
main().catch(error => {
  console.error("❌ Fatal error:", error);
  process.exit(1);
});
