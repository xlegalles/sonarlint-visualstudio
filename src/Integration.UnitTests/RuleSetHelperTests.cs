﻿//-----------------------------------------------------------------------
// <copyright file="RuleSetHelperTests.cs" company="SonarSource SA and Microsoft Corporation">
//   Copyright (c) SonarSource SA and Microsoft Corporation.  All rights reserved.
//   Licensed under the MIT License. See License.txt in the project root for license information.
// </copyright>
//-----------------------------------------------------------------------

using Microsoft.VisualStudio.CodeAnalysis.RuleSets;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.IO;
using System.Linq;

namespace SonarLint.VisualStudio.Integration.UnitTests
{
    [TestClass]
    public class RuleSetHelperTests
    {
        #region Tests
        [TestMethod]
        public void RuleSetHelper_RemoveAllIncludesUnderRoot()
        {
            // Setup
            const string slnRoot = @"X:\SolutionDir\";
            string projectRoot = Path.Combine(slnRoot, @"Project\");
            string sonarRoot = Path.Combine(slnRoot, @"Sonar\");
            string commonRoot = Path.Combine(slnRoot, @"Common\");

            const string sonarRs1FileName = "Sonar1.ruleset";
            const string sonarRs2FileName = "Sonar2.ruleset";
            const string projectRsBaseFileName = "ProjectBase.ruleset";
            const string commonRs1FileName = "SolutionCommon1.ruleset";
            const string commonRs2FileName = "SolutionCommon2.ruleset";

            var sonarRs1 = TestRuleSetHelper.CreateTestRuleSet(sonarRoot, sonarRs1FileName);
            var sonarRs2 = TestRuleSetHelper.CreateTestRuleSet(sonarRoot, sonarRs2FileName);
            var projectBaseRs = TestRuleSetHelper.CreateTestRuleSet(projectRoot, projectRsBaseFileName);
            var commonRs1 = TestRuleSetHelper.CreateTestRuleSet(commonRoot, commonRs1FileName);
            var commonRs2 = TestRuleSetHelper.CreateTestRuleSet(commonRoot, commonRs2FileName);

            var inputRuleSet = TestRuleSetHelper.CreateTestRuleSet(projectRoot, "test.ruleset");
            AddRuleSetInclusion(inputRuleSet, projectBaseRs, useRelativePath: true);
            AddRuleSetInclusion(inputRuleSet, commonRs1, useRelativePath: true);
            AddRuleSetInclusion(inputRuleSet, commonRs2, useRelativePath: false);
            AddRuleSetInclusion(inputRuleSet, sonarRs1, useRelativePath: true);
            AddRuleSetInclusion(inputRuleSet, sonarRs2, useRelativePath: false);

            var expectedRuleSet = TestRuleSetHelper.CreateTestRuleSet(projectRoot, "test.ruleset");
            AddRuleSetInclusion(expectedRuleSet, projectBaseRs, useRelativePath: true);
            AddRuleSetInclusion(expectedRuleSet, commonRs1, useRelativePath: true);
            AddRuleSetInclusion(expectedRuleSet, commonRs2, useRelativePath: false);

            // Act
            RuleSetHelper.RemoveAllIncludesUnderRoot(inputRuleSet, sonarRoot);

            // Verify
            RuleSetAssert.AreEqual(expectedRuleSet, inputRuleSet);
        }

        [TestMethod]
        public void RuleSetHelper_FindAllIncludesUnderRoot()
        {
            // Setup
            const string slnRoot = @"X:\SolutionDir\";
            string projectRoot = Path.Combine(slnRoot, @"Project\");
            string sonarRoot = Path.Combine(slnRoot, @"Sonar\");
            string commonRoot = Path.Combine(slnRoot, @"Common\");

            const string sonarRs1FileName = "Sonar1.ruleset";
            const string sonarRs2FileName = "Sonar2.ruleset";
            const string projectRsBaseFileName = "ProjectBase.ruleset";
            const string commonRs1FileName = "SolutionCommon1.ruleset";
            const string commonRs2FileName = "SolutionCommon2.ruleset";

            var sonarRs1 = TestRuleSetHelper.CreateTestRuleSet(sonarRoot, sonarRs1FileName);
            var sonarRs2 = TestRuleSetHelper.CreateTestRuleSet(sonarRoot, sonarRs2FileName);
            var projectBaseRs = TestRuleSetHelper.CreateTestRuleSet(projectRoot, projectRsBaseFileName);
            var commonRs1 = TestRuleSetHelper.CreateTestRuleSet(commonRoot, commonRs1FileName);
            var commonRs2 = TestRuleSetHelper.CreateTestRuleSet(commonRoot, commonRs2FileName);

            var inputRuleSet = TestRuleSetHelper.CreateTestRuleSet(projectRoot, "test.ruleset");
            AddRuleSetInclusion(inputRuleSet, projectBaseRs, useRelativePath: true);
            AddRuleSetInclusion(inputRuleSet, commonRs1, useRelativePath: true);
            AddRuleSetInclusion(inputRuleSet, commonRs2, useRelativePath: false);
            var expected1 = AddRuleSetInclusion(inputRuleSet, sonarRs1, useRelativePath: true);
            var expected2 = AddRuleSetInclusion(inputRuleSet, sonarRs2, useRelativePath: false);

            // Act
            RuleSetInclude[] actual = RuleSetHelper.FindAllIncludesUnderRoot(inputRuleSet, sonarRoot).ToArray();

            // Verify
            CollectionAssert.AreEquivalent(new[] { expected1, expected2 }, actual);
        }

        [TestMethod]
        public void RuleSetHelper_UpdateExistingProjectRuleSet()
        {
            // Setup
            const string existingProjectRuleSetPath = @"X:\MySolution\ProjectOne\proj1.ruleset";
            const string existingInclude = @"..\SolutionRuleSets\sonarqube1.ruleset";

            const string newSolutionRuleSetPath = @"X:\MySolution\SolutionRuleSets\sonarqube2.ruleset";
            const string expectedInclude = @"..\SolutionRuleSets\sonarqube2.ruleset";

            var existingProjectRuleSet = TestRuleSetHelper.CreateTestRuleSet(existingProjectRuleSetPath);
            existingProjectRuleSet.RuleSetIncludes.Add(new RuleSetInclude(existingInclude, RuleAction.Default));

            var expectedRuleSet = TestRuleSetHelper.CreateTestRuleSet(existingProjectRuleSetPath);
            expectedRuleSet.RuleSetIncludes.Add(new RuleSetInclude(expectedInclude, RuleAction.Default));

            // Act
            RuleSetHelper.UpdateExistingProjectRuleSet(existingProjectRuleSet, newSolutionRuleSetPath);

            // Verify
            RuleSetAssert.AreEqual(expectedRuleSet, existingProjectRuleSet, "Update should delete previous solution rulesets, and replace them with a new one provide");
        }

        [TestMethod]
        public void RuleSetHelper_FindInclude_ArgChecks()
        {
            RuleSet rs = new RuleSet("Name", @"c:\path.ruleset");

            Exceptions.Expect<ArgumentNullException>(() => RuleSetHelper.FindInclude(null, rs));
            Exceptions.Expect<ArgumentNullException>(() => RuleSetHelper.FindInclude(rs, null));
        }

        [TestMethod]
        public void RuleSetHelper_FindInclude()
        {
            // Setup
            RuleSet target = TestRuleSetHelper.CreateTestRuleSet(@"c:\Solution\SomeFolder\fullFilePath.ruleset");
            RuleSet sourceWithRelativeInclude = TestRuleSetHelper.CreateTestRuleSet(@"c:\fullFilePath.ruleset");
            string relativeInclude = @"Solution\SomeFolder\fullFilePath.ruleset".ToLowerInvariant(); // Catch casing errors
            sourceWithRelativeInclude.RuleSetIncludes.Add(new RuleSetInclude(relativeInclude, RuleAction.Error));
            RuleSet sourceWithAbsoluteInclude = TestRuleSetHelper.CreateTestRuleSet(@"c:\fullFilePath.ruleset");
            string absoluteInclude = target.FilePath.ToUpperInvariant(); // Catch casing errors
            sourceWithAbsoluteInclude.RuleSetIncludes.Add(new RuleSetInclude(absoluteInclude, RuleAction.Warning));
            RuleSetInclude include;

            // Case 1: Relative include
            // Act
            include = RuleSetHelper.FindInclude(sourceWithRelativeInclude, target);

            // Verify
            Assert.IsTrue(StringComparer.OrdinalIgnoreCase.Equals(include.FilePath, relativeInclude), $"Unexpected include {include.FilePath} instead of {relativeInclude}");

            // Case 2: Absolute include
            // Act
            include = RuleSetHelper.FindInclude(sourceWithAbsoluteInclude, target);

            // Verify
            Assert.IsTrue(StringComparer.OrdinalIgnoreCase.Equals(include.FilePath, absoluteInclude), $"Unexpected include {include.FilePath} instead of {absoluteInclude}");

            // Case 3: No includes at all
            // Act
            include = RuleSetHelper.FindInclude(target, target);
            // Verify
            Assert.IsNull(include, "No includes at all");

            // Case 4: No includes from source to target
            // Act
            include = RuleSetHelper.FindInclude(sourceWithRelativeInclude, sourceWithAbsoluteInclude);
            // Verify
            Assert.IsNull(include, "No includes from source to target");
        }
        #endregion

        #region Helpers

        private static RuleSetInclude AddRuleSetInclusion(RuleSet parent, RuleSet child, bool useRelativePath)
        {
            string include = useRelativePath
                ? PathHelper.CalculateRelativePath(parent.FilePath, child.FilePath)
                : child.FilePath;
            var ruleSetInclude = new RuleSetInclude(include, RuleAction.Default);
            parent.RuleSetIncludes.Add(ruleSetInclude);
            return ruleSetInclude;
        }

        #endregion
    }
}
