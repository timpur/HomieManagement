

namespace Homie.Models
{
    public class HomieOptions
    {
        public int QoSLevelSubscribe { get; set; }
        public int QoSLevelPublish { get; set; }
        public bool Retain { get; set; }

        public HomieOptions()
        {
            QoSLevelSubscribe = 0;
            QoSLevelPublish = 0;
            Retain = true;
        }
    }
}
