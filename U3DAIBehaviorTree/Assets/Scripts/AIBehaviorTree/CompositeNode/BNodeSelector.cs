﻿using System.Collections;
using System.Collections.Generic;


//  BNodeSelector.cs
//  Author: Lu Zexi
//  2014-06-07


namespace Game.AIBehaviorTree
{
    /// <summary>
    /// 选择器节点
    /// </summary>
    public class BNodeSelector : BNodeComposite
    {
		public BNodeSelector()
			:base()
		{
			this.m_strName = "sel";
		}

        /// <summary>
        /// 执行
        /// </summary>
        /// <param name="input"></param>
        /// <returns></returns>
		public override ActionResult Excute(BInput input)
        {
            for (int i = 0; i < this.m_lstChildren.Count; i++)
            {
				if (this.m_lstChildren[i].Excute(input) == ActionResult.SUCCESS)
					return ActionResult.SUCCESS;
            }
			return ActionResult.FAILURE;
        }
    }
}
