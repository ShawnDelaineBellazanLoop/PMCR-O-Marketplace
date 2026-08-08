$dll = 'C:\Users\org.tooensure\.nuget\packages\microsoft.agents.ai.workflows.declarative\1.17.0\lib\net472\Microsoft.Agents.AI.Workflows.Declarative.dll'
$asm = [System.Reflection.Assembly]::LoadFrom($dll)
$t = $asm.GetType('Microsoft.Agents.AI.Workflows.Declarative.ResponseAgentProvider')
Write-Host "IsAbstract:" $t.IsAbstract
foreach ($m in $t.GetMethods([System.Reflection.BindingFlags]::Public -bor [System.Reflection.BindingFlags]::Instance -bor [System.Reflection.BindingFlags]::DeclaredOnly)) {
  if ($m.IsAbstract -or $m.IsVirtual) {
    $params = ($m.GetParameters() | ForEach-Object { "$($_.ParameterType.Name) $($_.Name)" }) -join ', '
    Write-Host "$($m.ReturnType.Name) $($m.Name)($params) [abstract=$($m.IsAbstract)]"
  }
}
foreach ($c in $t.GetConstructors()) {
  $params = ($c.GetParameters() | ForEach-Object { "$($_.ParameterType.Name) $($_.Name)" }) -join ', '
  Write-Host "ctor($params)"
}
