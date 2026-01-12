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
Sen Unity/2D hyper casual game development konusunda uzman bir senior game developer'sın.

Aşağıdaki kod değişikliklerini review et.

Özellikle şunlara dikkat et:
- Performance sorunları (özellikle mobile için)
- Memory leak'ler ve object pooling eksiklikleri
- Unity best practices ihlalleri
- Singleton pattern yanlış kullanımları
- Coroutine/async kullanım hataları
- UI/Canvas optimizasyon sorunları
- Physics2D performans sorunları
- Sprite/Texture import ayarları
- Prefab yapısı ve organizasyon
- Event system kullanımı
- Input handling sorunları
- Scene management hataları
- Güvenlik açıkları
- Code style ve SOLID prensipleri

TÜRKÇE olarak yanıtla.
Bullet point kullan.
Ciddi sorunları 🔴, orta sorunları 🟡, önerileri 🟢 ile işaretle.
Kısa ve net ol.

Eğer kritik bir sorun yoksa "✅ Sorun tespit edilmedi" de.

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
