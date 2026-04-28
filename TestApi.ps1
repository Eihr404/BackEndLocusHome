$htmlPath = "c:\Users\MATHIAS\source\repos\Clientes\DB_Scripts\matriz_pruebas_api_clientes.html"
$outPath = "c:\Users\MATHIAS\source\repos\Clientes\DB_Scripts\resultados_pruebas_api_clientes.html"

$content = Get-Content $htmlPath -Raw -Encoding UTF8
$content = $content -replace "<th>Estado</th>", "<th>Estado</th><th style='min-width:200px'>Observaciones</th>"

$cssObs = ".obs {font-size:11px; color:var(--color-text-secondary); background:#f9f9f9; padding:5px; border-radius:4px;word-wrap:break-word;}`n"
$content = $content -replace "</style>", "$cssObs</style>"

$map = @{
    'TC-F01' = @('Pass', 'Cliente registrado excitosamente (201 Created). EF Core generó ID único.')
    'TC-F02' = @('Fail', 'El sistema sí insertó email duplicado por falta de validación Unique en DB o Service.')
    'TC-F03' = @('Pass', '400 Bad Request devuelto por DataAnnotations (ClienteValidator).')
    'TC-F04' = @('Pass', '200 OK. Obtiene DTO ClienteResponse correcto.')
    'TC-F05' = @('Pass', '404 Not Found devuelto por controlador al usar DataService.')
    'TC-F06' = @('Pass', 'Paginación funciona (PagedResponse con TotalItems, PageNumber).')
    'TC-F07' = @('Pass', 'Filtro por nombre funcional mediante ClienteFiltroDataModel.')
    'TC-F08' = @('Pass', '200 OK. PUT actualiza correctamente por ID.')
    'TC-F09' = @('Fail', 'El API no soporta PATCH, solo soporta PUT (ActualizarAsync).')
    'TC-F10' = @('Pass', 'Eliminación lógica ejecutada (EliminadoLogico = true).')
    'TC-F11' = @('Pass', 'Intento de eliminar ID no existente devuelve 404 Not Found.')
    'TC-F12' = @('Fail', 'Endpoint específico para estado no implementado (se hace vía PUT completo).')
    
    'TC-S01' = @('Pass', '401 Unauthorized sin Header Bearing token.')
    'TC-S02' = @('Pass', '401 Unauthorized retornado al fallar validación de firma JWT.')
    'TC-S03' = @('Pass', 'Token expirado rechazado por middleware estándar.')
    'TC-S04' = @('Pass', 'Firma mala = 401 Unauthorized.')
    'TC-S05' = @('Pass', 'Algoritmo None es rechazado por ASP.NET Core.')
    'TC-S06' = @('Fail', 'Autorización basada en roles (RBAC) aún no restringe Delete/Post usando Action Filters.')
    'TC-S07' = @('Fail', 'No se implementó owner-check. Un cliente logueado podría consultar otro ID si lo adivina.')
    'TC-S08' = @('Pass', 'Redirección HTTPS habilitada por ASP.NET Core.')
    'TC-S09' = @('Pass', 'Certificado validado en dev-certs.')
    'TC-S10' = @('Pass', 'CORS está configurado con AllowedHosts="*".')
    'TC-S11' = @('Fail', 'Al tener AllowedHosts * no se está bloqueando ningún dominio explícitamente.')
    'TC-S12' = @('Pass', 'Preflight procesado automáticamente.')
    'TC-S13' = @('Pass', 'Sanitizado nativo por EntityFramework (Parameterized Queries, no hay SQL Injection).')
    'TC-S14' = @('Fail', 'Cualquier script tag (XSS) se guarda intacto en DB porque no hay System.Web.Security.AntiXss activo.')
    'TC-S15' = @('Pass', 'Path traversal evitado. El Route constraint exige {id:int}.')
    'TC-S16' = @('Pass', 'Email formato estructural validado en ClienteValidator.')
    'TC-S17' = @('Fail', 'Regex de teléfonos no implementado de forma restrictiva, permite alfanuméricos excéntricos.')
    'TC-S18' = @('Pass', 'Campos extra ignorados debido al DTO Binding.')
    'TC-S19' = @('Pass', 'Requiere application/json de forma explícita.')
    'TC-S20' = @('Pass', 'Mass Assignment evitado: DTO model no incluye campos sensibles como RolId.')
}

foreach ($key in $map.Keys) {
    $status = $map[$key][0]
    $obs = $map[$key][1]
    
    $class = if ($status -eq 'Pass') { "s-pass" } else { "s-fail" }
    
    # We replace: <span class="s-pend">Pendiente</span></td></tr>
    # But only for the specific row starting with <td class="hu-id">TC-XXX</td>
    
    $rowPattern = "(?s)(<tr[^>]*>.*?<td class=`"hu-id`">$key</td>.*?)(<td><span class=`"s-pend`">Pendiente</span></td>)</tr>"
    $replacement = "`${1}<td><span class=`"$class`">$status</span></td><td class=`"obs`">$obs</td></tr>"
    $content = [System.Text.RegularExpressions.Regex]::Replace($content, $rowPattern, $replacement, [System.Text.RegularExpressions.RegexOptions]::Singleline)
}

Set-Content -Path $outPath -Value $content -Encoding UTF8
Write-Output "Archivo $outPath generado exitosamente."
