# Contributing to TeaShop

## Branch strategy
- `main` — Final branch, protected (requires 1 review + passing CI)
- `feature/<name>` — one feature per branch, branch from `main'

## Commit messages
Use the format: `type(scope): description`
Types: feat, fix, test, ci, docs, chore, refactor

## Pull requests
- At least 1 team member must review before merge
- CI pipeline must be green (build + tests + SAST passing)
- No hardcoded secrets, passwords, or connection strings — ever
