using System;
using UIForia.Layout;

namespace UIForia.Layout {

    public static class SpaceDistributionUtil {

        public static void GetAlignmentOffsets(float remaining, int contentCount, SpaceDistribution alignment, out float offset, out float spacerSize) {
            if (remaining <= 0) {
                offset = 0;
                spacerSize = 0;
                return;
            }

            offset = 0;
            spacerSize = 0;
            switch (alignment) {
                case SpaceDistribution.Unset:
                case SpaceDistribution.AfterContent:
                    break;
                case SpaceDistribution.CenterContent:
                    offset = remaining * 0.5f;
                    break;
                case SpaceDistribution.BeforeContent:
                    offset = remaining;
                    break;
                case SpaceDistribution.BetweenContent: {
                    if (contentCount == 1) {
                        offset = remaining * 0.5f;
                        break;
                    }

                    spacerSize = remaining / (contentCount - 1);
                    offset = 0;
                    break;
                }

                case SpaceDistribution.AroundContent: {
                    if (contentCount == 1) {
                        offset = remaining * 0.5f;
                        break;
                    }

                    spacerSize = (remaining / contentCount);
                    offset = spacerSize * 0.5f;
                    break;
                }

                default:
                    throw new ArgumentOutOfRangeException(nameof(alignment), alignment, null);
            }
        }

    }

}