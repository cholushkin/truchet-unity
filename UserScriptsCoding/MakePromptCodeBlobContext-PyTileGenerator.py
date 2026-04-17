import os, sys
sys.path.append(os.path.abspath(os.path.join(os.path.dirname(__file__), "../ScriptUtils")))
from Core.PromptContextCollector.PromptContextCollector import PromptContextCollector

PromptContextCollector(
    directories=["UserScriptsTileGenerator"],  # Wildcards and folders
    includes=["*.py", "*.md" ],
    files=[],
    ignores=[],
    template_path="UserScriptsCoding/TextTemplates/CodeBlobContext-PyTileGenerator.txt",  # Relative to project root
    template_vars={},
    output_path="UserScriptsCoding/Outputs/CodeBlobContext.txt"
).run()