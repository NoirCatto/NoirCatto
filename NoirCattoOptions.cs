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
        UseNoirStart = this.config.Bind<bool>("UseNoirStart", true);
        MeowKey = this.config.Bind<KeyCode>("MeowKey", KeyCode.LeftAlt);
    }

    public readonly Configurable<bool> AlternativeSlashConditions;
    public readonly Configurable<bool> UseNoirStart;
    public readonly Configurable<KeyCode> MeowKey;
    private UIelement[] UIArrOptions;
    private UIelement[] UIArrExtras;

    public override void Initialize()
    {
        var opTab = new OpTab(this, "Options");
        this.Tabs = new[]
        {
            opTab
        };

        UIArrOptions = new UIelement[]
        {
            new OpLabel(10f, 550f, "Main", true),
            new OpCheckBox(AlternativeSlashConditions, 10f, 520f),
            new OpLabel(40f, 520f, "Slash conditions: ") { verticalAlignment = OpLabel.LabelVAlignment.Center },
            
            new OpCheckBox(UseNoirStart, 10f, 490f),
            new OpLabel(40f, 490, "Custom Start (disable if Story Mode fails to load)") { verticalAlignment = OpLabel.LabelVAlignment.Center }
        };
        opTab.AddItems(UIArrOptions);
        
         UIArrExtras = new UIelement[]
         {
             new OpLabel(10f, 450f, "Fun and Extras", true){ color = new Color(0.65f, 0.85f, 1f) },
             new OpKeyBinder(MeowKey, new Vector2(10f, 420), new Vector2(150f, 30f), true, OpKeyBinder.BindController.AnyController),
             new OpLabel(166f, 420f, "Meow!") { verticalAlignment = OpLabel.LabelVAlignment.Center }
             
         };
        opTab.AddItems(UIArrExtras);
    }

    public override void Update()
    {
        ((OpLabel)UIArrOptions[2]).text = "Slash conditions: " + (!((OpCheckBox)UIArrOptions[1]).GetValueBool() ?
            "Default - Empty hands, or no directional input while holding an object" : 
            "Alternative - Main hand must be empty" );
    }
    
    
}