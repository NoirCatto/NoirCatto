using System.Collections.Generic;
using System.Linq;
using BepInEx.Logging;
using JollyCoop;
using Menu.Remix.MixedUI;
using Menu.Remix.MixedUI.ValueTypes;
using MoreSlugcats;
using UnityEngine;

namespace NoirCatto;

public class NoirCattoOptions : OptionInterface
{
    public NoirCattoOptions()
    {
        NoirAltSlashConditions = config.Bind(nameof(NoirAltSlashConditions), false);
        NoirBuffSlash = config.Bind(nameof(NoirBuffSlash), false);
        NoirAutoSlash = config.Bind(nameof(NoirAutoSlash), false);
        NoirDisableAutoCrouch = config.Bind(nameof(NoirDisableAutoCrouch), false);
        NoirUseCustomStart = config.Bind(nameof(NoirUseCustomStart), NoirCatto.CustomStartMode.Story);
        NoirAttractiveMeow = config.Bind(nameof(NoirAttractiveMeow), true);
        NoirHideEars = config.Bind(nameof(NoirHideEars), false);
        NoirMeowKey = config.Bind(nameof(NoirMeowKey), KeyCode.LeftAlt);
    }

    public readonly Configurable<bool> NoirAltSlashConditions;
    public readonly Configurable<bool> NoirBuffSlash;
    public readonly Configurable<bool> NoirAutoSlash;
    public readonly Configurable<bool> NoirDisableAutoCrouch;
    public readonly Configurable<NoirCatto.CustomStartMode> NoirUseCustomStart;
    public readonly Configurable<bool> NoirAttractiveMeow;
    public readonly Configurable<bool> NoirHideEars;
    public readonly Configurable<KeyCode> NoirMeowKey;
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
            new OpCheckBox(NoirAltSlashConditions, 10f, 520f),
            new OpLabel(40f, 520f, "Slash conditions: ") { verticalAlignment = OpLabel.LabelVAlignment.Center },
            new OpCheckBox(NoirAutoSlash, 10f, 490f),
            new OpLabel(40f, 490f, "Auto slash (when holding down throw button)") { verticalAlignment = OpLabel.LabelVAlignment.Center },
            new OpCheckBox(NoirBuffSlash, 10f, 460f),
            new OpLabel(40f, 460f, "Buff slash (2x damage and stun time)") { verticalAlignment = OpLabel.LabelVAlignment.Center },

            new OpCheckBox(NoirDisableAutoCrouch, 10f, 430f),
            new OpLabel(40f, 430f, "Disable automatic crouch") { verticalAlignment = OpLabel.LabelVAlignment.Center },

            new BetterComboBox(NoirUseCustomStart, new Vector2(10f, 400f), 150, OpResourceSelector.GetEnumNames(null, typeof(NoirCatto.CustomStartMode)).ToList()),
            new OpLabel(166f, 400f, "Where to use Noir's custom start (disable if breaks story or expedition)") { verticalAlignment = OpLabel.LabelVAlignment.Center },
        };
        opTab.AddItems(UIArrOptions);

        var offset = 105f; //yes I'm lazy
        UIArrExtras = new UIelement[]
        {
            new OpLabel(10f, 450f - offset, "Fun and Extras", true){ color = new Color(0.65f, 0.85f, 1f) },
            new OpKeyBinder(NoirMeowKey, new Vector2(10f, 420f - offset), new Vector2(150f, 30f), true, OpKeyBinder.BindController.AnyController),
            new OpLabel(166f, 420f - offset, "Meow!") { verticalAlignment = OpLabel.LabelVAlignment.Center },
            new OpCheckBox(NoirAttractiveMeow, 10f, 390f - offset),
            new OpLabel(40f, 390f - offset, "Creatures react to meows") { verticalAlignment = OpLabel.LabelVAlignment.Center },
            new OpCheckBox(NoirHideEars, 10f, 360f - offset),
            new OpLabel(40f, 360f - offset, "Hide ears (eg. For use with DMS)") { verticalAlignment = OpLabel.LabelVAlignment.Center },
        };
        opTab.AddItems(UIArrExtras);
    }

    public override void Update()
    {
        ((OpLabel)UIArrOptions[2]).text = "Slash conditions: " + (!((OpCheckBox)UIArrOptions[1]).GetValueBool() ?
            "Default - Empty hands, or no directional input while holding an object" :
            "Alternative - Main hand must be empty" );

        var box = (BetterComboBox)UIArrOptions[9];
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