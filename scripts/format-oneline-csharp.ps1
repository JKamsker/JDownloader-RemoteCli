$ErrorActionPreference = "Stop"

function Should-ReformatFile([string] $content) {
    if ([string]::IsNullOrWhiteSpace($content)) {
        return $false
    }

    if ($content -notmatch "namespace\s+[^;\r\n]+;\s+(public|internal)\s+(sealed\s+)?class") {
        return $false
    }

    # Heuristic: some generated/templated files end up as a single mega-line after the usings.
    $lines = $content -split "\r?\n"
    return ($lines | Where-Object { $_.Length -gt 200 }).Count -gt 0
}

function Expand-CSharp([string] $content) {
    $nl = [Environment]::NewLine

    # Ensure file-scoped namespace isn't glued to the class declaration.
    $content = [Regex]::Replace(
        $content,
        "namespace(\s+[^;\r\n]+;)\s+(?=(public|internal)\s+)",
        "namespace`$1$nl$nl",
        [Text.RegularExpressions.RegexOptions]::Multiline)

    $builder = New-Object System.Text.StringBuilder
    $inString = $false
    $inChar = $false
    $escape = $false
    $verbatim = $false
    $inLineComment = $false
    $inBlockComment = $false

    $len = $content.Length
    for ($i = 0; $i -lt $len; $i++) {
        $c = $content[$i]
        $next = if ($i + 1 -lt $len) { $content[$i + 1] } else { [char]0 }

        if ($inLineComment) {
            [void]$builder.Append($c)
            if ($c -eq "`n") { $inLineComment = $false }
            continue
        }

        if ($inBlockComment) {
            [void]$builder.Append($c)
            if ($c -eq "*" -and $next -eq "/") {
                [void]$builder.Append($next)
                $i++
                $inBlockComment = $false
            }
            continue
        }

        if ($inString) {
            [void]$builder.Append($c)
            if ($verbatim) {
                if ($c -eq '"' -and $next -eq '"') {
                    [void]$builder.Append($next)
                    $i++
                } elseif ($c -eq '"') {
                    $inString = $false
                    $verbatim = $false
                }
            } else {
                if ($escape) {
                    $escape = $false
                } elseif ($c -eq "\") {
                    $escape = $true
                } elseif ($c -eq '"') {
                    $inString = $false
                }
            }
            continue
        }

        if ($inChar) {
            [void]$builder.Append($c)
            if ($escape) {
                $escape = $false
            } elseif ($c -eq "\") {
                $escape = $true
            } elseif ($c -eq "'") {
                $inChar = $false
            }
            continue
        }

        # comment starts
        if ($c -eq "/" -and $next -eq "/") {
            [void]$builder.Append($c).Append($next)
            $i++
            $inLineComment = $true
            continue
        }
        if ($c -eq "/" -and $next -eq "*") {
            [void]$builder.Append($c).Append($next)
            $i++
            $inBlockComment = $true
            continue
        }

        # string starts
        if ($c -eq "@" -and $next -eq '"') {
            [void]$builder.Append($c).Append($next)
            $i++
            $inString = $true
            $verbatim = $true
            continue
        }
        if ($c -eq '"') {
            [void]$builder.Append($c)
            $inString = $true
            $verbatim = $false
            continue
        }
        if ($c -eq "'") {
            [void]$builder.Append($c)
            $inChar = $true
            continue
        }

        if ($c -eq ";") {
            [void]$builder.Append($c)
            if ($next -ne "`r" -and $next -ne "`n") {
                [void]$builder.Append($nl)
            }
            continue
        }

        if ($c -eq "{") {
            [void]$builder.Append($c)
            if ($next -ne "`r" -and $next -ne "`n") {
                [void]$builder.Append($nl)
            }
            continue
        }

        if ($c -eq "}") {
            if ($builder.Length -gt 0) {
                $last = $builder[$builder.Length - 1]
                if ($last -ne "`n" -and $last -ne "`r") {
                    [void]$builder.Append($nl)
                }
            }

            [void]$builder.Append($c)
            if ($next -ne "`r" -and $next -ne "`n" -and $next -ne [char]0) {
                [void]$builder.Append($nl)
            }
            continue
        }

        [void]$builder.Append($c)
    }

    $result = $builder.ToString()
    if (-not $result.EndsWith($nl)) {
        $result += $nl
    }

    return $result
}

$repoRoot = (Resolve-Path (Join-Path $PSScriptRoot "..")).Path
$targets =
    Get-ChildItem -Path (Join-Path $repoRoot "src") -Recurse -File -Filter "*.cs" |
    Where-Object { $_.FullName -notmatch "\\\\(bin|obj)\\\\" }

$changed = 0
foreach ($file in $targets) {
    $content = Get-Content -Raw -Path $file.FullName
    if (-not (Should-ReformatFile $content)) { continue }

    $updated = Expand-CSharp $content
    if ($updated -ne $content) {
        Set-Content -Path $file.FullName -Value $updated -NoNewline -Encoding utf8NoBOM
        $changed++
    }
}

if ($changed -gt 0) {
    Write-Host "Reformatted $changed one-line C# file(s)."
}
