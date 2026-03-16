# github-repository-explorer-243913-243927

## .NET runner for Python `hello_world.py`

This repo includes a minimal .NET console app that runs `hello_world.py` via the system Python executable and prints stdout/stderr.

### Prerequisites
- .NET SDK (8.0+ recommended)
- Python available on `PATH` (`python3` or `python`)

### Run
From the repo root:

```bash
dotnet run --project dotnet_py_runner
```

Optional overrides:

```bash
dotnet run --project dotnet_py_runner -- --python python3 --script hello_world.py
```
