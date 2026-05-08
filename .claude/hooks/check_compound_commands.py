#!/usr/bin/env python3
"""
PreToolUse hook: Bash で &&, || などの複合コマンドを検出してブロックする。
許可プロンプトが毎回出るのを防ぐため、複合コマンドは禁止し、
コマンドを分割して個別に実行するよう促す。
"""

import json
import re
import sys


def main() -> None:
    try:
        data = json.load(sys.stdin)
    except json.JSONDecodeError:
        sys.exit(0)

    command = data.get("tool_input", {}).get("command", "")

    if re.search(r"&&|\|\|", command):
        result = {
            "hookSpecificOutput": {
                "hookEventName": "PreToolUse",
                "permissionDecision": "deny",
                "permissionDecisionReason": (
                    "複合コマンド (&&, ||) は使用禁止です。"
                    "コマンドを1つずつ分割して個別に実行してください。"
                ),
            }
        }
        print(json.dumps(result, ensure_ascii=False))


if __name__ == "__main__":
    main()
