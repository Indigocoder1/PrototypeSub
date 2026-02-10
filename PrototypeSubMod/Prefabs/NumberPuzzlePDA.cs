using System.Collections.Generic;
using System.Linq;
using Nautilus.Assets;
using Nautilus.Assets.Gadgets;
using Nautilus.Assets.PrefabTemplates;
using Nautilus.Handlers;
using PrototypeSubMod.Compatibility;
using PrototypeSubMod.Registration;
using SuitLib;
using UnityEngine;

namespace PrototypeSubMod.Prefabs;

public static class NumberPuzzlePDA
{
    public static PrefabInfo PrefabInfo { get; private set; }

    public static void Register()
    {
        PrefabInfo = PrefabInfo.WithTechType("NumberPuzzlePDA", null, null, "English");

        var prefab = new CustomPrefab(PrefabInfo);
        var template = new CloneTemplate(PrefabInfo, "c6f6fe72-e16e-4b00-8df2-6b4e1a3533f4");
        template.ModifyPrefab += gameObject =>
        {
            var sht = gameObject.GetComponent<StoryHandTarget>();
            sht.goal.key = "NumberPuzzlePDA";
        };

        prefab.SetGameObject(template);

        prefab.Register();
    }
}