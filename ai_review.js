#!/usr/bin/env node

import fs from "fs";
import fetch from "node-fetch";

const diff = fs.readFileSync("diff.txt", "utf8");

if (!diff.trim()) {
  console.log("No diff found, skipping review.");
  process.exit(0);
}

const prompt = `
You are a senior software engineer.

Review the following GitHub Pull Request diff.

Focus on:
- Bugs
- Security issues
- Performance
- Code style
- Architecture problems

Reply in Turkish.
Use bullet points.
Be strict but constructive.

DIFF:
${diff}
`;

const response = await fetch("https://openrouter.ai/api/v1/chat/completions", {
  method: "POST",
  headers: {
    "Authorization": `Bearer ${process.env.OPENROUTER_API_KEY}`,
    "Content-Type": "application/json",
    "HTTP-Referer": "https://github.com",
    "X-Title": "AI Code Review Bot"
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
  process.exit(1);
}

const data = await response.json();
const review = data?.choices?.[0]?.message?.content;

if (!review) {
  console.error("AI review failed: No content in response");
  console.error(JSON.stringify(data, null, 2));
  process.exit(1);
}

// PR numarasını bul - GitHub event payload'dan al
const eventPath = process.env.GITHUB_EVENT_PATH;
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

const repo = process.env.GITHUB_REPOSITORY;

// PR'a comment at
const commentResponse = await fetch(
  `https://api.github.com/repos/${repo}/issues/${prNumber}/comments`,
  {
    method: "POST",
    headers: {
      "Authorization": `Bearer ${process.env.GITHUB_TOKEN}`,
      "Content-Type": "application/json",
      "Accept": "application/vnd.github+json"
    },
    body: JSON.stringify({ body: review })
  }
);

if (!commentResponse.ok) {
  console.error(`GitHub API error: ${commentResponse.status} ${commentResponse.statusText}`);
  const errorText = await commentResponse.text();
  console.error(errorText);
  process.exit(1);
}

console.log("AI review comment posted.");