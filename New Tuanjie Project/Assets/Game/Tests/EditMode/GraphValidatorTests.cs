using System.Collections.Generic;
using FMBG.AI;
using NUnit.Framework;
using UnityEditor;
using UnityEngine;

namespace FMBG.Tests
{
    /// <summary>T6 状态图校验器。</summary>
    public class GraphValidatorTests
    {
        [Test]
        public void T6_DefaultGraph_Passes()
        {
            var graph = AssetDatabase.LoadAssetAtPath<EnemyStateGraph>(
                "Assets/Game/Configs/Graphs/Enemy_DefaultGraph.asset");
            Assert.IsNotNull(graph, "默认状态图应存在");

            List<string> errors = EnemyGraphValidator.Validate(graph);
            Assert.IsEmpty(errors, "默认图应校验通过，但存在错误: " + string.Join("; ", errors));
        }
    }
}
