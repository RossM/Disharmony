# Repository Instructions

General:
- Exercise judgment about the user’s intent. Treat hypothetical, conditional, and exploratory remarks as context rather than authorization. Before making consequential changes to architecture, public APIs, persisted data, compatibility, scope, or user-visible behavior—or acting on an ambiguous, inconsistent, or materially mistaken premise—explain the concern and confirm the direction. Resolve minor ambiguities conservatively within the stated scope.
- Preserve CRLF line endings in files edited in this repository.

For commits:
- Add a `Codex-Assisted: true` trailer to every commit created by Codex, separated from the subject/body by a blank line. Preserve the configured Git author identity unless the user requests otherwise.

For test code:
- Preserve reasonable tests that expose possible product bugs. Report the failing behavior and suspected code path to the user instead of changing the test merely to make it pass.
- If a test fixture may be invalid or compiler-dependent, investigate and explain the evidence before changing it. Keep distinct valid code paths covered separately.
- Keep test cases self-contained. Avoid helper methods unless they would be used by 10 or more test cases.

For non-test code:
- Implement the simplest cohesive design that accurately represents the problem domain. Use types and structures that make invariants explicit and invalid states difficult to represent; avoid loosely related fields or parameters, conditionally valid state, duplicated logic, and unnecessary abstraction.
- Treat test failures as diagnostic evidence, not merely conditions to satisfy. Identify the underlying rule or design flaw, and do not add fixture-specific behavior or special cases unless they represent genuine domain distinctions. As requirements become clearer, reshape an inadequate initial design as though those requirements had been known from the start. Passing tests is necessary but does not by itself establish that the implementation is sound.
- Keep changes proportionate to the requested task. Perform cleanup needed to leave modified code coherent, but report material opportunities outside the task rather than expanding its scope. If a sound solution requires a consequential redesign, explain the diagnosis and design options and obtain approval before proceeding; do not substitute a brittle workaround merely to avoid that discussion.
- Write code that communicates its structure and intent clearly. Use comments to explain non-obvious intent, constraints, or behavior rather than restating the code. Correct misleading comments in code you modify and report significant problems elsewhere without expanding the task.

For documentation:
- Before creating or revising documentation, read and follow [Writing Disharmony documentation](docs/writing-documentation.md). It defines the audiences, landing-page admission criteria, and destinations for detailed material.
- Write for the reader's task and assumed knowledge. Explain unfamiliar patching concepts in context; distinguish hypothetical game source from code readers should put in their mods.
- Keep the root README and Workshop description focused on their entry questions. Put detailed instructions in the appropriate sub-page and link to them; keep Disharmony-specific load-order and Harmony advice on Workshop, and put generic installation help on the HTML player page.
- Do not put progress reports, test counts, investigation notes, temporary decisions, or planning checklists in public user documentation. Keep planning checklists outside source control.
- Include implementation details only when they help the intended reader complete the task. Internal contributor documentation can explain design and mechanics; user guides should explain observable behavior and restrictions.
- Edit source Markdown/XML comments, not generated documentation. Check placement, context, and links against the guide before finishing.
