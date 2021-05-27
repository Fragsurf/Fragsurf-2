/**
 * RectTransformEx.cs
 * 
 * @author mosframe / https://github.com/mosframe
 * 
 */

namespace Mosframe
{
    using UnityEngine;

    /// <summary>
    /// RectTransform Extention
    /// </summary>
    public static class RectTransformEx
    {
        /// <summary>
        /// 최대 사이즈로 설정
        /// </summary>
        public static RectTransform setFullSize( this RectTransform self ) {

            self.sizeDelta  = new Vector2(0.0f,0.0f);
            self.anchorMin  = new Vector2(0.0f,0.0f);
            self.anchorMax  = new Vector2(1.0f,1.0f);
            self.pivot      = new Vector2(0.5f,0.5f);
            return self;
        }
        
        /// <summary>
        /// 사이즈 얻기 
        /// </summary>
        public static Vector2 getSize( this RectTransform self ) {
            return self.rect.size;
        }

        /// <summary>
        /// 사이즈 설정 
        /// </summary>
        public static void setSize( this RectTransform self, Vector2 newSize ) {

            var pivot   = self.pivot;
            var dist    = newSize - self.rect.size;
            self.offsetMin = self.offsetMin - new Vector2( dist.x * pivot.x, dist.y * pivot.y );
            self.offsetMax = self.offsetMax + new Vector2( dist.x * (1f - pivot.x), dist.y * (1f - pivot.y) );
        }
       
        /// <summary>
        /// 좌측기준으로 크기설정
        /// </summary>
        public static RectTransform setSizeFromLeft( this RectTransform self, float rate ) {

            self.setFullSize();

            var width = self.rect.width;

            self.anchorMin  = new Vector2(0.0f,0.0f);
            self.anchorMax  = new Vector2(0.0f,1.0f);
            self.pivot      = new Vector2(0.0f,1.0f);
            self.sizeDelta  = new Vector2(width*rate,0.0f);

            return self;
        }
        
        /// <summary>
        /// 우측기준으로 크기설정
        /// </summary>
        public static RectTransform setSizeFromRight( this RectTransform self, float rate ) {

            self.setFullSize();

            var width = self.rect.width;

            self.anchorMin  = new Vector2(1.0f,0.0f);
            self.anchorMax  = new Vector2(1.0f,1.0f);
            self.pivot      = new Vector2(1.0f,1.0f);
            self.sizeDelta  = new Vector2(width*rate,0.0f);

            return self;
        }

        /// <summary>
        /// 상단기준으로 크기설정
        /// </summary>
        public static RectTransform setSizeFromTop( this RectTransform self, float rate ) {

            self.setFullSize();

            var height = self.rect.height;

            self.anchorMin  = new Vector2(0.0f,1.0f);
            self.anchorMax  = new Vector2(1.0f,1.0f);
            self.pivot      = new Vector2(0.0f,1.0f);
            self.sizeDelta  = new Vector2(0.0f,height*rate);

            return self;
        }

        /// <summary>
        /// 하단 기준으로 크기설정
        /// </summary>
        public static RectTransform setSizeFromBottom( this RectTransform self, float rate ) {

            self.setFullSize();

            var height = self.rect.height;

            self.anchorMin  = new Vector2(0.0f,0.0f);
            self.anchorMax  = new Vector2(1.0f,0.0f);
            self.pivot      = new Vector2(0.0f,0.0f);
            self.sizeDelta  = new Vector2(0.0f,height*rate);

            return self;
        }
        
        /// <summary>
        /// 오프셋 설정 
        /// </summary>
        public static void setOffset( this RectTransform self, float left, float top, float right, float bottom ) {

            self.offsetMin = new Vector2( left, top );
            self.offsetMax = new Vector2( right, bottom );
        }

        /// <summary>
        /// 스크린 좌표가 Rect안에 포함되는지 확인
        /// </summary>
        public static bool inScreenRect( this RectTransform self, Vector2 screenPos ) {

            var canvas = self.GetComponentInParent<Canvas>();
            switch( canvas.renderMode )
            {
            case RenderMode.ScreenSpaceCamera:
                {
                    var camera = canvas.worldCamera;
                    if( camera != null )
                    {
                        return RectTransformUtility.RectangleContainsScreenPoint( self, screenPos, camera );
                    }
                }
                break;
            case RenderMode.ScreenSpaceOverlay:
                return RectTransformUtility.RectangleContainsScreenPoint( self, screenPos );
            case RenderMode.WorldSpace:
                return RectTransformUtility.RectangleContainsScreenPoint( self, screenPos );
            }
            return false;
        }
        
        /// <summary>
        /// 다른 RectTransform이 RectTransform안에 포함되는지 확인
        /// </summary>
        public static bool inScreenRect( this RectTransform self, RectTransform rectTransform ) {

            var rect1 = getScreenRect( self );
            var rect2 = getScreenRect( rectTransform );
            return rect1.Overlaps( rect2 );
        }
        
        /// <summary>
        /// 스크린좌표 Rect를 얻는다.
        /// </summary>
        public static Rect getScreenRect( this RectTransform self ) {

            var rect = new Rect();
            var canvas = self.GetComponentInParent<Canvas>();
            var camera = canvas.worldCamera;
            if( camera != null )
            {
                var corners = new Vector3[4];
                self.GetWorldCorners( corners );
                rect.min = camera.WorldToScreenPoint( corners[0] );
                rect.max = camera.WorldToScreenPoint( corners[2] );
            }
            return rect;
        }
    }
}