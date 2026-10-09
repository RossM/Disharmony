# Writing Disharmony documentation

Read this guide before creating or revising documentation. It defines the intended readers, the purpose of each kind of page, and the rules for deciding where information belongs. Paths below are relative to the repository root.

## Choose the reader and task

Before writing, identify who the page is for, what they should be able to do after reading it, and what knowledge it assumes.

| Reader | First useful resource | Guidance needed |
| --- | --- | --- |
| Players using a dependent mod | Workshop installation instructions or player help | Explicit subscription, enabling, and load-order steps for a short installation task. |
| Beginners new to C# and programming | One primary C# tutorial, with prerequisites and a return point | Help choosing where to learn; assume basic programming knowledge in the Disharmony tutorials. |
| C# readers new to patching | Runtime-patching introduction, then a complete first patch | Explain separate game/mod assemblies, targets, registration, prefixes, and postfixes before relying on them. |
| Experienced Harmony users | Concrete comparison, then an adoption example | Explain practical benefits, limitations, and coexistence; let them skip the beginner introduction. |
| Authors already using Disharmony | Task-based example, API entry, or diagnostic explanation | Precise contracts and corrections, searchable lookup, and links between related material. |
| Contributors | Development/testing guide, then relevant internal design material | Build and validation instructions plus orientation to the code and its constraints. |

Readers may arrive directly through search, Discord, NuGet, or an IDE diagnostic. Give enough context for the page's own task, explain unfamiliar patching concepts before using them, and link to the next useful step. Clearly distinguish hypothetical game-method source from code readers should put in their mods.

## Match the page to the task

- Tutorials lead readers through a working result and explain new concepts before relying on them.
- Usage examples answer a concrete question with original behavior, target source, patch source, and expected results.
- API reference pages describe contracts, defaults, restrictions, and related examples.
- Troubleshooting starts with what readers observe, then gives a cause and correction.
- Contributor material explains development and internal design where those details help someone make a change.

User-facing documentation should not carry incidental planning notes or implementation details that do not help its readers. Internal contributor documentation can explain implementation details where they aid a change.

## Content placement and landing-page criteria

These rules apply to future edits as well as the initial rewrite. A landing page helps its reader recognize the product, decide whether it is relevant, and choose a next step. It is not the default destination for newly discovered information.

### Admission test for landing-page content

Every paragraph, list item, or snippet must answer at least one entry question for that page's audience:

1. What is Disharmony, and what does it do for me?
2. Do I need it, or does it help with the kind of mod I want to write?
3. Can I use it with my game/runtime, and is its published maturity relevant to that decision?
4. What must I do now to install it or reach the appropriate learning/lookup resource?
5. Where do I get help or contribute?

If content does not answer one of those questions, place it in the relevant sub-page or planning record. If answering the question requires a tutorial, several commands, a detailed table, or implementation explanation, keep a brief answer and a descriptive link on the landing page; put the explanation on the destination page.

A detail may be promoted from a sub-page when omitting it would cause a likely installation mistake or materially mislead a reader's initial decision. State the effect on the reader, then link to the detail. Do not promote implementation mechanics merely because they are technically important.

Public product status belongs here only when it affects use: for example, that the published package is a beta and APIs may change. Work status belongs in the local checklist: which tests just passed, which warning was fixed, what was investigated, or what remains on an agent's worklist.

### Root README.md: allowed content and shape

Primary readers are mod authors discovering Disharmony; players and contributors need short, visible routes to their destinations.

Include, in this order unless presentation warrants a small adjustment:

1. A short explanation of the framework and its distinguishing capability: patching operations inside methods as well as whole methods.
2. A compact explanation of why an author might use it, with a link to the concrete Harmony comparison. Do not reproduce the full comparison.
3. A brief published-beta/support statement and a link to compatibility. Summarize supported library runtimes; keep the full matrix and dependency version rules on the compatibility page.
4. Descriptive entry links for programming beginners, C# readers new to patching, the first Disharmony patch, Harmony users, and existing users seeking examples/API/diagnostics.
5. A short player route to the Workshop item or installation help.
6. A contribution link, an appropriate support/reporting link, and a license link.

A single package-install command is acceptable if it makes the author's next step easier. Full code examples and procedures belong in the tutorials. Repository layout should not be required to understand the links; use names such as "Write your first patch" rather than relying on a project directory name to explain the destination.

Do not put these in the root README:

- SDK setup, solution build/test commands, runtime-runner flags, or test fixture details.
- Package-build commands, version-override instructions, deployment scripts, or publishing procedures.
- Test counts, build logs, benchmark results from an investigation, completed-work summaries, release checklists, or future task lists.
- Patch registration internals, locking mechanics, generated IL details, compiler/extractor workarounds, or historical reasons for project configuration.
- Full analyzer catalogs, API signatures, deployment XML, long compatibility tables, or hypothetical target-method source.

### Steam Workshop description: allowed content and shape

Primary readers are players arriving because another mod requires Disharmony. The description must be sufficient for the ordinary installation task without requiring a visit to GitHub. Its source is `Disharmony.RimWorld/About/README.md`.

The `<description>` in `Disharmony.RimWorld/About/About.xml` is separate: players see it in the in-game mod manager when enabling, disabling, and sorting installed mods. Keep it short and focused on the mod's purpose, whether to enable it, and load order; subscription instructions belong on Workshop.

Include:

1. A brief explanation that it supplies a framework used by other mods, and how a player knows they need it.
2. Supported public RimWorld version information that affects whether the player can use it.
3. Subscription, enabling, dependency, and load-order steps required to use it.
4. Brief advice for the most common installation mistake, with a link to more detailed player help.
5. A clearly labelled "For mod authors" link to the author documentation.
6. A support/reporting route where appropriate.

Explain bundled assemblies or Harmony coexistence only to the extent needed to answer a player action or compatibility question. Do not include .NET target-framework terminology, NuGet installation, patch syntax, build instructions, development status reports, or internal design. Keep release history in Workshop change notes rather than accumulating it in the description.

### Other landing pages

- The HTML home page routes the six audiences to their first useful page and keeps API search/reference visible. It does not repeat the tutorials, diagnostic catalog, or contributor setup.
- The NuGet README serves authors who have found the package. Include the install command, compact library/runtime and analyzer-host information needed to use the package, the RimWorld deployment distinction, and direct links to the detailed guides. Do not include maintainer packaging procedures.
- Sub-pages can also act as entry points from search or Discord. Give them enough context to understand their own task, but do not repeat the entire product introduction or all audience routes.

### Where detailed information belongs

These are logical destinations, not a requirement to create a separate file for every row. Reuse existing guides where they serve the same reader and task; add a page when the audience, task, or prerequisites differ enough to warrant it. Existing source files named here remain the source of their generated HTML equivalents.

| Information | Destination | Boundary |
| --- | --- | --- |
| C# learning resources and readiness to begin | Programming-prerequisites page | One primary learning route, prerequisite concepts, and the next step. |
| How separate mod assemblies change game behavior | Runtime-patching introduction | Explain the model with a small method; distinguish explanatory game source from mod source. |
| Package installation and first working patch | Getting-started guide, currently `Disharmony/README.md` | Complete source and activation instructions; use assumed C# knowledge. |
| Why choose Disharmony and comparison with transpilers | Harmony comparison | Same target and result in both implementations; practical benefits and limitations. |
| Adding Disharmony to an existing Harmony project | Adoption example | Concrete changes and registration, with links to deployment and coexistence guidance. |
| Task-specific patch examples | `docs/usage-examples.md` | Target source, desired change, patch, explanation, expected behavior, and reference links. |
| API contracts, overloads, defaults, constraints, exceptions | Public XML comments/generated API reference | Implementation details only when needed to understand an observable restriction; worked procedures belong in guides. |
| Full runtime support matrix and Harmony dependency policy | `docs/compatibility.md` | Canonical support information; landing pages carry only the relevant summary. |
| Analyzer host requirements, setup, severity, configuration, and diagnostic corrections | `Disharmony.Analyzers/README.md` and linked diagnostic sections/pages | Explain what the author observes and how to correct it; analyzer implementation belongs in contributor material. |
| RimWorld author setup, registration location, dependency XML, and deployment | `Disharmony.RimWorld/README.md` and suitable setup links | Author instructions; player subscription/help belongs in the player route. |
| Player installation and troubleshooting | Workshop description plus short HTML player help | Keep ordinary installation self-contained on Workshop; link to longer symptom-based help. |
| Building Disharmony, SDK requirements, and supported-runtime tests | Contributor entry/testing guide, currently `Disharmony.Tests/README.md` | Commands and validation expectations for source changes, not instructions to compile an ordinary dependent mod. |
| Internal subsystems, concurrency mechanisms, IL generation, and design constraints | Contributor architecture overview | Explain details that help locate or safely change the implementation. |
| Editing/building the docs and generator workarounds | `docs/README.md` | Maintainer instructions; consumer-facing generated-signature explanations stay in `docs/extension-members.md`. |
| NuGet packing and release tooling | Contributor/maintainer build or release instructions | Reproducible procedures when useful to contributors; the private execution checklist stays outside source control. |
| Changes in a published version | GitHub release notes and Workshop change notes | Describe resulting user behavior and relevant limitations, not a transcript of development. |
| Release readiness, task progress, test-run results, investigations, and temporary decisions | Local release checklist, this plan, issue records, or local logs as appropriate | Do not move these into public user guides. An actionable product defect can have an issue; summarize a user-relevant limitation in its guide when needed. |

Keep one authoritative home for detailed facts. Repeat only the short facts required at a different entry point, and link to the authoritative explanation. Update those summaries when the underlying support or deployment facts change.

### Concrete placement examples

- "Disharmony is available as a beta" can belong on the root README because it affects adoption. "We have finished NuGet packaging" belongs in the local release checklist.
- "Install and enable Disharmony before the mod that requires it" belongs in player installation instructions. `About.xml` dependency entries belong in the author deployment guide.
- "Use the net472 library for Mono" belongs in compatibility guidance and may appear as a short author-facing summary. The reason reference projects use a particular SDK or framework belongs in contributor setup when relevant to building them.
- "An error handler must not add or remove patches" is a user-visible API restriction and belongs in the relevant API/usage documentation. The thread-local flag implementing the restriction belongs in code or contributor design material.
- A command to build the library, a command to pack NuGet, and the DocFX extraction workaround do not belong in the root README; put them in contributor build, maintainer packaging, and documentation-build instructions respectively.
- A test count from the last run belongs in the local validation record. It does not establish a permanent product property for the README.

### Landing-page review gate

Before adding or keeping content on a landing page:

- Identify its reader and the entry question it answers.
- Verify it meets that specific page's allowed content above.
- Check whether a short answer plus a descriptive link would suffice.
- Confirm that ordinary installation steps remain directly available to players on Workshop.
- Check that its claims describe the published product, rather than reporting current work.
- Place necessary detail in its canonical sub-page; remove incidental detail instead of moving it indiscriminately into another public page.
- Check that destination links work and their titles explain what the reader will find.
