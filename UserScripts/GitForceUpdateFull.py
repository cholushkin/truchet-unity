import os, sys, subprocess


sys.path.append(os.path.abspath(os.path.join(os.path.dirname(__file__), "../ScriptUtils")))
from Core.GitSubmoduleManager.GitSubmoduleManager import GitSubmoduleManager


# update main repo
subprocess.run(["git", "pull"], check=True)

# force update submodules
GitSubmoduleManager().force_update_all_submodules()