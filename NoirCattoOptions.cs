using System.Collections.Generic;
using System.Linq;
using BepInEx.Logging;
using Menu.Remix.MixedUI;
using Menu.Remix.MixedUI.ValueTypes;
using MoreSlugcats;
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
        WorldState = this.config.Bind<SlugcatStats.Name>("WorldState", NoirCatto.GourmandName);
    }

    public readonly Configurable<bool> AlternativeSlashConditions;
    public readonly Configurable<bool> UseNoirStart;
    public readonly Configurable<KeyCode> MeowKey;
    public readonly Configurable<SlugcatStats.Name> WorldState;
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
            new OpLabel(40f, 490f, "Custom Start (disable if Story Mode fails to load)") { verticalAlignment = OpLabel.LabelVAlignment.Center },
            
            new OpLabel(10f, 460f, "World State") { verticalAlignment = OpLabel.LabelVAlignment.Center },
            new BetterComboBox(WorldState, new Vector2(10f, 430f), 200f, OpResourceSelector.GetEnumNames(null, typeof(SlugcatStats.Name)).ToList()), //TODO: Remove jollycoopplayers n shit
            new OpLabel(220f, 430f, "(default: Gourmand, Hunter if MSC is not present)") { verticalAlignment = OpLabel.LabelVAlignment.Center }
        };
        opTab.AddItems(UIArrOptions);

        var offset = 100f; //yes I'm lazy
        UIArrExtras = new UIelement[]
        {
            new OpLabel(10f, 450f - offset, "Fun and Extras", true){ color = new Color(0.65f, 0.85f, 1f) },
            new OpKeyBinder(MeowKey, new Vector2(10f, 420f - offset), new Vector2(150f, 30f), true, OpKeyBinder.BindController.AnyController),
            new OpLabel(166f, 420f - offset, "Meow!") { verticalAlignment = OpLabel.LabelVAlignment.Center }
        };
        opTab.AddItems(UIArrExtras);
    }

    public override void Update()
    {
        ((OpLabel)UIArrOptions[2]).text = "Slash conditions: " + (!((OpCheckBox)UIArrOptions[1]).GetValueBool() ?
            "Default - Empty hands, or no directional input while holding an object" : 
            "Alternative - Main hand must be empty" );

        var box = (BetterComboBox)UIArrOptions[6];
        if (box.IsOpen)
        {
            foreach (var element in UIArrExtras)
            {
                element.Hide();
            }
        }
        else
        {
            foreach (var element in UIArrExtras)
            {
                element.Show();
            }
        }
    }
    
    //Thanks Henpe
    class BetterComboBox : OpComboBox
    {
        public BetterComboBox(ConfigurableBase configBase, Vector2 pos, float width, List<ListItem> list) : base(configBase, pos, width, list) { }

        public bool IsOpen;
        public override void GrafUpdate(float timeStacker)
        {
            base.GrafUpdate(timeStacker);
            if(this._rectList != null && !_rectList.isHidden)
            {
                for (int j = 0; j < 9; j++)
                {
                    this._rectList.sprites[j].alpha = 1;
                }

                IsOpen = true;
            }
            else
            {
                IsOpen = false;
            }
        }
    }
    
    
}