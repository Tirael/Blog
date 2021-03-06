﻿namespace Examples.Tests.Windows
{
    using System;

    using Examples.Windows;

    using Microsoft.VisualStudio.TestTools.UnitTesting;

    [TestClass]
    public class ClipboardHelperTests
    {
        [TestMethod]
        public void TextTest()
        {
            string guid = Guid.NewGuid().ToString();
            ClipboardHelper.SetText(guid);
            Assert.AreEqual(guid, ClipboardHelper.GetText());
        }
    }
}
