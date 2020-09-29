/*
 * This file is part of ZTransport, copyright ©2020 BloodyRum.
 *
 * ZTransport is free software: you can redistribute it and/or modify it under
 * the terms of the GNU General Public License as published by the Free Soft-
 * ware Foundation, either version 3 of the License, or (at your option) any
 * later version.
 *
 * ZTransport is distributed in the hope that it will be useful, but WITHOUT
 * ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FIT-
 * NESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more
 * details.
 *
 * You should have received a copy of the GNU General Public License along with
 * ZTransport. If not, see <https://www.gnu.org/licenses/>.
 */

using UnityEngine;

namespace ZTransport {
    // plop this here baby into any machine that doesn't already have a
    // component that sets its animations correctly, and it'll set you up real
    // nice! -SB, drunk or asleep
    public class OperationalGlueShimThing : KMonoBehaviour, ISim1000ms {
#pragma warning disable 0649
        [MyCmpReq]
        private Operational operational;
        [MyCmpReq]
        private KAnimControllerBase animator;
#pragma warning restore 0649
        bool was_operational = false;
        public void Sim1000ms(float dt) {
            bool is_operational = operational.IsOperational;
            if(is_operational != was_operational) {
                animator.Play(is_operational ? (HashedString)"on" : (HashedString)"off", KAnim.PlayMode.Once, 1f, 0f);
                was_operational = is_operational;
            }
        }
    }
}
