# Contributing to TeaShop

## Branch strategy
- `main` — Final branch, protected (requires 1 review + passing CI)
- `feature/<name>` — one feature per branch, branch from `main'

## Commit messages
Use the format: `type(scope): description`
types:
- `feat`: A new feature
- `fix`: A bug fix
- `test`: Adding or updating tests
- `ci`: Changes to GitHub Actions/Workflow
- `docs`: Documentation changes
- `refactor`: Code changes that neither fix a bug nor add a feature

## Pull requests
- At least 1 team member must review before merge
- CI pipeline must be green (build + tests + SAST passing)
- No hardcoded secrets, passwords, or connection strings — ever
