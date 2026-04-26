import re

with open("Assets/Scenes/pruebas.unity", "r", encoding="utf-8") as f:
    content = f.read()

lines = content.split('\n')
objects = {}
current_id = None
current_type = None
current_lines = []

for line in lines:
    m = re.match(r'--- !u!(\d+) &(\d+)', line)
    if m:
        if current_id:
            objects[current_id] = {'type': current_type, 'lines': current_lines}
        current_type = m.group(1)
        current_id = m.group(2)
        current_lines = []
    else:
        if current_id:
            current_lines.append(line)

if current_id:
    objects[current_id] = {'type': current_type, 'lines': current_lines}

name_to_id = {}
id_to_name = {}
for fid, obj in objects.items():
    if obj['type'] == '1':
        for line in obj['lines']:
            m = re.search(r'm_Name: (.+)', line)
            if m:
                name = m.group(1).strip()
                if name not in name_to_id:
                    name_to_id[name] = fid
                id_to_name[fid] = name
                break

def get_components(go_id):
    obj = objects.get(go_id)
    if not obj: return []
    result = []
    for l in obj['lines']:
        if 'component:' in l:
            m = re.search(r'fileID: (\d+)', l)
            if m: result.append(m.group(1))
    return result

def get_mb_fields(mb_id):
    obj = objects.get(mb_id)
    if not obj: return {}, ''
    fields = {}
    script = ''
    for line in obj['lines']:
        if 'm_Script' in line: script = line.strip()
        stripped = line.strip()
        if ': ' in stripped:
            k, v = stripped.split(': ', 1)
            fields[k.strip()] = v.strip()
    return fields, script

TYPE_NAMES = {
    '1':'GameObject','4':'Transform','114':'MonoBehaviour','65':'BoxCollider',
    '23':'MeshRenderer','33':'MeshFilter','54':'Rigidbody','82':'AudioSource',
    '135':'SphereCollider','143':'CapsuleCollider','224':'RectTransform',
    '225':'CanvasRenderer','222':'Canvas'
}

SKIP_FIELDS = {
    'm_ObjectHideFlags','m_CoroutineID','m_StopAction','m_EditorHideFlags',
    'm_EditorClassIdentifier','m_Enabled','m_Script','serializedVersion',
    'm_PrefabInstance','m_PrefabAsset','m_GameObject','m_LocalRotation',
    'm_LocalPosition','m_LocalScale','m_Children','m_Father','m_RootOrder',
    'm_LocalEulerAnglesHint','m_TagString','m_Icon','m_NavMeshLayer',
    'm_StaticEditorFlags','m_IsActive','m_Layer','m_Name','m_Mesh',
    'm_Materials','m_CastShadows','m_ReceiveShadows','m_DynamicOccludee',
    'm_StaticShadowCaster','m_MotionVectors','m_LightProbeUsage',
    'm_ReflectionProbeUsage','m_RayTracingMode','m_RayTraceProcedural',
    'm_RenderingLayerMask','m_RendererPriority','m_LightmapIndex',
    'm_LightmapTilingOffset','m_LightmapParameters','m_SortingLayerID',
    'm_SortingLayer','m_SortingOrder','m_AdditionalVertexStreams',
}

targets = [
    'CodePuzzleManager','Scolor','Sobjeto','Sbool','Soperador','Sfuncion',
    'azul','rojo','verde','amarillo','==','!=','abrir','cerrar','true','false','llave','mesa',
    'llaves','slots','palabras','puerta','puerta2','puertas','puertas (1)','CodeWordsSpawner'
]

for name in targets:
    fid = name_to_id.get(name)
    if not fid:
        print(f"\n[{name}] *** NOT FOUND ***")
        continue
    print(f"\n{'='*55}")
    print(f"  GO: {name}  (fileID={fid})")
    for cid in get_components(fid):
        obj = objects.get(cid)
        if not obj: continue
        t = obj['type']
        tname = TYPE_NAMES.get(t, f'type={t}')
        if t == '114':
            fields, script = get_mb_fields(cid)
            print(f"  >> MonoBehaviour")
            print(f"       script: {script}")
            for k, v in fields.items():
                if k not in SKIP_FIELDS and v not in ('0','','{}','- []','[]'):
                    print(f"       {k}: {v}")
        else:
            print(f"  >> {tname}")
