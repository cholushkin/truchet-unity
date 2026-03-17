import os, sys
sys.path.append(os.path.abspath(os.path.join(os.path.dirname(__file__), "../ScriptUtils")))
from Core.PromptContextCollector.PromptContextCollector import PromptContextCollector

PromptContextCollector(
    directories=["Unity/TruchetTiles/Assets/Core"],  # Wildcards and folders
    includes=["*.cs", "*.md", "*.shader"],
    files=["readme.md"],
    ignores=[],
    template_path="UserScriptsCoding/TextTemplates/CodeBlobContext.txt",  # Relative to project root
    template_vars={},
    output_path="UserScriptsCoding/Outputs/CodeBlobContext.txt"
).run()