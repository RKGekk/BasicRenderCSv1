using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BasicRender.Phis {

    public interface IParticleForceGenerator {

        void updateForce(Particlef particle, float duration);
    }
}
