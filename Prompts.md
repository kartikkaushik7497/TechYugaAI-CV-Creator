Got it — let’s make your testing efficient and thorough. Below is a practical QA checklist, plus sample prompts you can use to validate AI, UI, and PDF behavior quickly.

#### 

#### ✅ Quick Testing Checklist (Manual QA)

1\. General Navigation \& Modes

Switch between General Chat / CV Creator / Bid Maker / Proposals.

Confirm mode state resets correctly.

Check New Chat button works and clears chat + preview.

2\. Guided CV Mode

Start empty → no preview content.

Answer questions step by step.

Confirm live preview updates after each response.

Verify template switching doesn’t scroll or reset form.

3\. Prompt + Job Description

Leave prompt empty → validation modal shows.

Fill prompt → generate resume.

Confirm preview updated \& buttons appear.

Test template change after PDF generated → should auto‑refresh.

4\. Resume Upload Mode

Upload PDF, verify file label + extracted length shown.

Prompt empty → validation modal.

Valid prompt → Improve Resume works.

Preview shows updated resume.

Generate PDF → view/download appears.

5\. PDF Generation

Generate PDF twice → no crash / no duplicate errors.

Check PDF download opens correct file.

Ensure PDF layout matches preview (spacing, headings).

6\. Dark / Light Mode

Toggle icon → verify entire UI changes (toolbar, cards, inputs).

Check preview is always readable (white page).

Chat input text visible in dark mode.

7\. Suggestion Buttons

Click “What skills should I include?” → should answer, not overwrite draft.

Resume draft should remain unchanged.





##### ✅ Sample Prompts (Copy/Paste)



Please generate a professional, ATS‑optimized one‑page resume for a React Developer with 3 years of experience.
Skills: React 18, TypeScript, Redux, Tailwind CSS, Jest.
Experience: 3 years at a SaaS startup working on dashboards and UI performance.
Include projects and a strong summary.

Prompt Mode (No Experience)



Generate a resume for a Fresher Front‑End Developer (0 years experience).
Skills: HTML, CSS, JavaScript, React, Git.
Projects: portfolio website, weather app.
Do not include any professional experience.

Resume Upload Mode



Improve this resume for a Node.js Developer role.
Highlight backend performance, scalable APIs, and MongoDB optimization.
Emphasize reliability and architecture.

Guided CV Mode (use in chat)



My full name is Kartik Kaushik.
I’m a Node.js Developer with 1 year experience at Techpro.
Skills: Node, Express, MongoDB, React.
Project: Ecommerce dashboard.
Education: MCA 2024‑2026.





#### ✅ UI Focused Test Steps

Use this short flow to check layout quickly:



Open CV Creator.

Switch all 3 tabs.

Change templates 3–4 times.

Toggle dark/light.

Resize browser (mobile width, tablet).

Generate PDF → verify buttons appear.

If you want, I can also create a formal QA test case document for your final semester submission (with test IDs + expected results).

