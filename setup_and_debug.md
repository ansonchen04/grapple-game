# Startup Instructions

To setup the LLM after a reboot or if it says the backend isn't connected, run the following in the command line:

1. Open PowerShell and enter WSL:
```powershell
   wsl
```
2. Navigate to the project and launch:
```bash
   cd ~/club-3090
   bash scripts/launch.sh
```

Then, just say yes to all the prompts.

# Debugging Aider

## Git Repo Corrupted

For some reason, sometimes Aider will make a mistake on a commit or something, and then it will be unable to make any other changes (Aider saves every single one of its changes as a commit so you can easily `/undo` any mistakes it makes). If it happens to do this, an easy fix is as follows:

1. Go to the `grapple-game/.git/objects` folder. If you're navigating via file explorer, you may need to enable the "show hidden folders" option.
2. Delete the `pack` folder.
3. Go to the terminal, navigate to your `grapple-game` folder, and run:
```bash
   git fetch origin
```

Please note, this will also get rid of any changes since the last push. Be sure to save your changes frequently!