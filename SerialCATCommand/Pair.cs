
using System.Collections.Generic;
using System.Windows.Markup;

namespace SerialCATCommand {
    [ContentProperty("Content")]
    public class AngleObject {
        public double Angle { get; set; }

        public object Content { get; set; }
    }

    public class AngleObjectList : List<AngleObject> {

    }
}
