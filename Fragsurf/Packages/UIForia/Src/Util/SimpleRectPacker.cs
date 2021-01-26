using System;

namespace UIForia.Util {

    public class SimpleRectPacker {

        private int totalWidth;
        private int totalHeight;
        private int padding;

        private static int s_IdGenerator;

        private readonly StructList<PackedRect> rectList;
        private readonly StructList<PackedRect> possibleCollisionList;

        public SimpleRectPacker(int totalWidth, int totalHeight, int padding) {
            this.totalWidth = totalWidth;
            this.totalHeight = totalHeight;
            this.padding = padding;
            this.rectList = new StructList<PackedRect>();
            this.possibleCollisionList = new StructList<PackedRect>();
        }

        public void Reset(int totalWidth, int totalHeight, int padding) {
            this.rectList.size = 0;
            this.totalWidth = totalWidth;
            this.totalHeight = totalHeight;
            this.padding = padding;
        }

        public void Clear() {
            rectList.size = 0;
        }
        
        public void RemoveRect(int id) {
            for (int i = 0; i < rectList.size; i++) {
                if (rectList.array[i].id == id) {
                    rectList.SwapRemoveAt(i);
                    return;
                }
            }
        }

        public int checks = 0;
        
        public bool TryPackRect(int width, int height, out PackedRect retn) {
            width += padding;
            height += padding;

            retn = new PackedRect();

            if (width > totalWidth || height > totalHeight) {
                retn = default;
                return false;
            }


            retn.xMax = width;
            retn.yMax = height;

        //   possibleCollisionList.EnsureCapacity(rectList.size);

            PackedRect[] packedRects = rectList.array;

         //   Array.Copy(rectList.array, 0, packedRects, 0, rectList.size);
           // possibleCollisionList.size = rectList.size;
        
            // can keep sorted list by x coord
            // can't change regions
            // basically want to only check things we MIGHT collide with
            // best packing = move over by min x of collision set
            
            int xMin = int.MaxValue;

            while (true) {
                int intersectCount = 0;
                int yMax = int.MinValue;

                int rectCount = rectList.size;

                for (int i = 0; i < rectCount; i++) {
                    ref PackedRect check = ref packedRects[i];

                    if(check.xMax <= retn.xMin || check.yMax <= retn.yMin) continue;
                    checks++;

                    bool intersects = !(retn.yMin >= check.yMax ||
                                      retn.yMax <= check.yMin ||
                                      retn.xMax <= check.xMin ||
                                      retn.xMin >= check.xMax);

                    if (intersects) {
                        intersectCount++;
                        if (check.xMax < xMin) xMin = check.xMax;
                        if (check.yMax > yMax) yMax = check.yMax;
                    }
                }

                if (intersectCount == 0) {
                    retn.id = ++s_IdGenerator;
                    rectList.Add(retn);
                    return true;
                }

                retn.yMin = yMax;
                retn.yMax = retn.yMin + height;

                if (retn.yMax > totalHeight) {
                    retn.yMin = 0;
                    retn.yMax = height;
                    retn.xMin += (xMin - retn.xMin);
                    retn.xMax = retn.xMin + width;
                    xMin = int.MaxValue;

                    if (retn.xMax > totalWidth) {
                        retn = default;
                        return false;
                    }

                    // if a rect cannot ever intersect, stop checking this by removing it from the list or mark as do not check
//                    for (int i = 0; i < possibleCollisionList.size; i++) {
//                        if (packedRects[i].xMax < retn.xMin) {
//                            packedRects[i--] = packedRects[--possibleCollisionList.size];
//                        }
//                    }
                }
            }
        }

        public struct PackedRect {

            public int id;
            public int xMin;
            public int yMin;
            public int xMax;
            public int yMax;

        }


    }

}