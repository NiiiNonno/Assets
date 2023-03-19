using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Nonno.Assets.Collections;

namespace Nonno.Assets.Test;
[TestClass]
public class WordCollectionTest
{
    [TestMethod]
    public void Escape()
    {
        string SUBJECT = $@"""A'B""'""'""C'D E'F""'""'""G'H'I""'""'""J'K""";

        var list = new WordList(SUBJECT);
        Assert.AreEqual($@"A'B""C'D E'F""G'H'I""J'K", string.Join(" ", list));

        Assert.AreEqual(SUBJECT, WordSpan.Unescape(WordSpan.Escape(SUBJECT)));
    }
}
