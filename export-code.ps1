# === Настройки =============================================

# Путь к корню проекта (фиксируем, чтобы запускать из любого места)
$RootPath = "C:\Pet-Project\OnlineStore"        # <-- твой проект

# Куда писать результат (единый файл с кодом и конфигами)
$OutFile  = Join-Path $RootPath "project-code.txt"

# Папки, которые нужно исключить из обхода (типичный «шум» .NET/Node)
$ExcludeDirs = @('bin','obj','node_modules','.git','.vs','dist','coverage','coveragereport','TestResults')

# Маски файлов, содержимое которых нужно собрать
$IncludePatterns = @('*.cs','*.csproj','appsettings*.json')

# === Подготовка ============================================

# Очищаем файл результата (или создаём заново) в кодировке UTF-8
"" | Out-File -FilePath $OutFile -Encoding utf8

# Превратим пути «исключаемых» папок в удобный для сравнения набор (строки в нижнем регистре)
$ExcludeSet = [System.Collections.Generic.HashSet[string]]::new([System.StringComparer]::OrdinalIgnoreCase)
$ExcludeDirs | ForEach-Object { $ExcludeSet.Add($_) | Out-Null }

# === Функция проверки, лежит ли элемент внутри исключённых папок ===
function Test-IsInExcludedFolder {
    param(
        [Parameter(Mandatory=$true)][string]$FullPath,
        [Parameter(Mandatory=$true)][string]$Root
    )
    # Получаем относительный путь от корня, заменяем слэши на одинаковые, режем по папкам
    $rel = $FullPath.Substring($Root.Length).TrimStart('\','/')
    $parts = $rel -split '[\\/]' | Where-Object { $_ -ne '' }

    # Если в любом сегменте встретилась папка из $ExcludeSet — считаем «исключён»
    foreach ($p in $parts) {
        if ($ExcludeSet.Contains($p)) { return $true }
    }
    return $false
}

# === Логика обхода и записи =================================

# Для каждой маски (*.cs, *.csproj, appsettings*.json) собираем файлы
foreach ($pattern in $IncludePatterns) {

    # Рекурсивно берём все файлы по шаблону
    Get-ChildItem -Path $RootPath -Recurse -File -Filter $pattern -Force |
        Where-Object {
            # Отфильтруем всё, что попало внутрь исключённых папок
            -not (Test-IsInExcludedFolder -FullPath $_.FullName -Root $RootPath)
        } |
        Sort-Object FullName |              # Стабильный порядок: удобно читать/искать
        ForEach-Object {
            $file = $_

            # --- Заголовок файла (чёткий разделитель между файлами)
            "===== FILE: $($file.FullName) =====" | Out-File -FilePath $OutFile -Append -Encoding utf8

            try {
                # Пишем содержимое файла в UTF-8
                Get-Content -LiteralPath $file.FullName -Encoding utf8 |
                    Out-File -FilePath $OutFile -Append -Encoding utf8
            }
            catch {
                # На случай, если какой-то файл с «чудной» кодировкой — отметим ошибку и идём дальше
                "[[WARN]] Cannot read file as UTF-8: $($file.FullName) -- $($_.Exception.Message)" |
                    Out-File -FilePath $OutFile -Append -Encoding utf8
            }

            # Пустая строка-разделитель между файлами
            "" | Out-File -FilePath $OutFile -Append -Encoding utf8
        }
}

Write-Host "Готово! Сохранён файл: $OutFile"
