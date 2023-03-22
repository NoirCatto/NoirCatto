using BepInEx.Logging;
using Menu.Remix.MixedUI;
using Menu.Remix.MixedUI.ValueTypes;
using UnityEngine;

namespace NoirCatto;

public class NoirCattoOptions : OptionInterface
{
    public readonly ManualLogSource Logger;
    public NoirCattoOptions(NoirCatto modInstance, ManualLogSource loggerSource)
    {
        Logger = loggerSource;
        AlternativeSlashConditions = this.config.Bind<bool>("AlternativeSlashConditions", false);
    }

    public readonly Configurable<bool> AlternativeSlashConditions;
    private UIelement[] UIArrOptions;
    //private UIelement[] UIArrRambo;

    public override void Initialize()
    {
        var opTab = new OpTab(this, "Options");
        this.Tabs = new[]
        {
            opTab
        };

        UIArrOptions = new UIelement[]
        {
            new OpLabel(10f, 550f, "Combat", true),
            new OpCheckBox(AlternativeSlashConditions, 10f, 520f),
            new OpLabel(40f, 520f, "Slash conditions: ") { verticalAlignment = OpLabel.LabelVAlignment.Center }
        };
        opTab.AddItems(UIArrOptions);

        // UIArrRambo = new UIelement[]
        // {
        //     new OpLabel(10f, 470f, "Rambo", true){color = new Color(0.65f, 0.1f, 0.1f)},
        //     new OpCheckBox(StartWithBombs, 10f, 440f),
        //     new OpLabel(40f, 440f, "Start cycle with BombBelt resupplied"),
        //     
        //     new OpLabel(10f, 410f, "BombBelt capacity (default = 3)"),
        //     new OpUpdown(BeltCapacity, new Vector2(10f, 380f), 100f),
        //     
        // };
        //opTab.AddItems(UIArrRambo);
    }

    public override void Update()
    {
        ((OpLabel)UIArrOptions[2]).text = "Slash conditions: " + (!((OpCheckBox)UIArrOptions[1]).GetValueBool() ?
            "Default - Empty hands, or no directional input while holding an object" : 
            "Alternative - Main hand must be empty" );
    }
    
    
}