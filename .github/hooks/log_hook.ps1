param(
	[Parameter(Mandatory = $true)]
	[string]$type
)

$raw = [Console]::In.ReadToEnd()
$text = $null

if ($raw) {
	try {
		$payload = $raw | ConvertFrom-Json

		if ($payload.prompt) {
			$text = [string]$payload.prompt
		} elseif ($payload.userPrompt) {
			$text = [string]$payload.userPrompt
		} elseif ($payload.input) {
			$text = [string]$payload.input
		} elseif ($payload.message) {
			if ($payload.message.content -is [string]) {
				$text = [string]$payload.message.content
			} elseif ($payload.message.content.text) {
				$text = [string]$payload.message.content.text
			}
		} elseif ($payload.messages) {
			$last = $payload.messages | Select-Object -Last 1
			if ($last.content -is [string]) {
				$text = [string]$last.content
			} elseif ($last.content.text) {
				$text = [string]$last.content.text
			}
		}
	} catch {
		$text = $null
	}
}

if (-not $text) {
	if ($raw) {
		$text = $raw.Trim()
	} else {
		$text = '(no stdin)'
	}
}

$logPath = Join-Path -Path $PSScriptRoot -ChildPath "..\..\lab-1\agent_log.txt"
Add-Content -LiteralPath $logPath -Value ("{0}: {1}" -f $type, $text)