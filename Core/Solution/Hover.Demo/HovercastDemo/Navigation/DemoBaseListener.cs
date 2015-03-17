﻿using Hover.Cast.Custom;
using Hover.Cast.Items;
using UnityEngine;

namespace Hover.Demo.HovercastDemo.Navigation {

	/*================================================================================================*/
	public abstract class DemoBaseListener<T> : HovercastItemListener<T> where T : NavItem {

		protected DemoEnvironment Enviro { get; private set; }
		protected HovercastCustomizationProvider Custom { get; private set; }
		protected SegmentSettings SegSett { get; private set; }
		protected InteractionSettings InteractSett { get; private set; }


		////////////////////////////////////////////////////////////////////////////////////////////////
		/*--------------------------------------------------------------------------------------------*/
		protected override void Setup() {
			const string env = "DemoEnvironment";

			Enviro = GameObject.Find(env).GetComponent<DemoEnvironment>();
			Custom = GameObject.Find(env+"/MenuData").GetComponent<HovercastCustomizationProvider>();
			SegSett = Custom.GetSegmentSettings(null);
			InteractSett = Custom.GetInteractionSettings();
		}

	}

}
