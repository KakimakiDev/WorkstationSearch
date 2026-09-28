param([Parameter(Mandatory=$true)][string]$GameManaged,[Parameter(Mandatory=$true)][string]$BepInExCore,[Parameter(Mandatory=$true)][string]$PluginDll)
$ErrorActionPreference='Stop'
foreach($name in @('Mono.Cecil.dll','MonoMod.Utils.dll','MonoMod.RuntimeDetour.dll','0Harmony.dll','BepInEx.dll')){[Reflection.Assembly]::LoadFrom((Join-Path $BepInExCore $name)) | Out-Null}
foreach($name in @('UnityEngine.CoreModule.dll','UnityEngine.UI.dll','UnityEngine.UIModule.dll','UnityEngine.dll','Unity.TextMeshPro.dll','assembly_utils.dll','assembly_guiutils.dll','assembly_valheim.dll')){[Reflection.Assembly]::LoadFrom((Join-Path $GameManaged $name)) | Out-Null}
$plugin=[Reflection.Assembly]::LoadFrom($PluginDll)
$flags=[Reflection.BindingFlags]'Static,NonPublic'
$listener=[HarmonyLib.AccessTools]::Method([UnityEngine.Events.UnityEvent],'AddListener',[Type[]]@([UnityEngine.Events.UnityAction]))
if(-not $listener -or $listener.GetParameters().Length -ne 1){throw 'UnityAction overload not resolved'}
$instantiate=[UnityEngine.Object].GetMethods() | Where-Object {$_.Name -eq 'Instantiate' -and $_.IsGenericMethodDefinition -and $_.GetParameters().Length -eq 2 -and $_.GetParameters()[1].ParameterType -eq [UnityEngine.Transform]}
$instantiate=$instantiate.MakeGenericMethod([Type[]]@([UnityEngine.GameObject]))
$instructions=New-Object 'System.Collections.Generic.List[HarmonyLib.CodeInstruction]'
$instructions.Add((New-Object HarmonyLib.CodeInstruction([Reflection.Emit.OpCodes]::Call,$instantiate)))
$instructions.Add((New-Object HarmonyLib.CodeInstruction([Reflection.Emit.OpCodes]::Callvirt,$listener)))
$transpiler=$plugin.GetType('WorkstationSearch.CraftRowCreation').GetMethod('Transpiler',$flags)
$arguments=New-Object object[] 1
$arguments[0]=$instructions.psobject.BaseObject
$output=@($transpiler.Invoke($null,$arguments))
foreach($name in @('GetRow','AddClick')){if(@($output | Where-Object {$_.operand -is [Reflection.MethodInfo] -and $_.operand.Name -eq $name}).Count -ne 1){throw "Missing $name hook"}}
$ready=$plugin.GetType('WorkstationSearch.CraftRowReuse').GetField('CreationHookReady',$flags)
if(-not $ready.GetValue($null)){throw 'Hook was disabled'}
# Unsupported method shapes must leave the original calls intact and disable reuse.
$instructions.Clear()
$instructions.Add((New-Object HarmonyLib.CodeInstruction([Reflection.Emit.OpCodes]::Call,$instantiate)))
$instructions.Add((New-Object HarmonyLib.CodeInstruction([Reflection.Emit.OpCodes]::Callvirt,$listener)))
$instructions.Add((New-Object HarmonyLib.CodeInstruction([Reflection.Emit.OpCodes]::Callvirt,$listener)))
$arguments=New-Object object[] 1
$arguments[0]=$instructions.psobject.BaseObject
$output=@($transpiler.Invoke($null,$arguments))
if($ready.GetValue($null) -or @($output | Where-Object {$_.operand -eq $listener}).Count -ne 2){throw 'Unsupported shape did not fall back'}
'Unity AddListener overload, actual creation transpiler, and unsupported-shape fallback checks passed.'
$original=[UnityEngine.UI.ScrollRect].GetMethod('OnScroll',[Type[]]@([UnityEngine.EventSystems.PointerEventData]))
$prefix=$plugin.GetType('WorkstationSearch.RecipeViewportWheel').GetMethod('Prefix',$flags)
foreach($parameter in $prefix.GetParameters()) {
    if($parameter.Name -eq '__instance') {
        if($parameter.ParameterType -ne $original.DeclaringType){throw 'Incorrect scroll instance binding'}
        continue
    }
    if($parameter.Name -match '^__(\d+)$') {
        $index=[int]$Matches[1]
        $targets=$original.GetParameters()
        if($index -ge $targets.Length -or $parameter.ParameterType -ne $targets[$index].ParameterType){throw 'Incorrect positional scroll binding'}
    } else {
        $target=@($original.GetParameters() | Where-Object {$_.Name -eq $parameter.Name -and $_.ParameterType -eq $parameter.ParameterType})
        if($target.Count -ne 1){throw "Unbound Harmony scroll argument: $($parameter.Name)"}
    }
}
'Scroll prefix argument bindings verified against the installed Unity method.'
