---
description: "Validate a packaged skill against Claude Code's own plugin schema before it's registered. Usage: /skill-creator:validate-skill <path-to-package>"
---
```
package_path: <first argument, relative to repo_path>
```

Run `claude plugin validate .` at `repo_path` -- Claude Code's own
marketplace/plugin schema (naming, `source` paths, category values).

If this repo also maintains a `catalog/skills.schema.json`, validate the
package's prospective entry against that too and report both results
explicitly, even if one clearly failed and checking the other feels
redundant -- a package that's schema-valid in one system but not the
other is exactly the kind of gap a two-check rule exists to catch.
