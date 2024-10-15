# RunElevated

This repository contains the Windows tool "RunElevated" which allows you to run any application elevated.

Usage:

```shell
> RunElevated [--wait] <Program> [ProgramArgs]
```

If `--wait` is specified, RunElevated will wait for the specified program to finish and return its exit code.

If `--wait` is *not* specified, RunElevated will start the specified program and then exit immediately (with exit code `0`).
