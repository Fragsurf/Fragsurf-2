using UIForia.Elements;
using UIForia.Rendering;
using UnityEngine;

namespace UIForia {

    public struct MaterialInterface {

        private UIElement element;
        private Application application;

        public MaterialInterface(UIElement element, Application application) {
            this.element = element;
            this.application = application;
        }

        public void SetFloat(string materialName, string propertyName, float value) {
            application.materialDatabase.SetInstanceProperty(element.id, materialName, propertyName, new MaterialPropertyValue2() {
                floatValue = value
            });
        }

        public void SetFloat(MaterialId materialId, string propertyName, float value) {
            application.materialDatabase.SetInstanceProperty(element.id, materialId, propertyName, new MaterialPropertyValue2() {
                floatValue = value
            });
        }

        public void SetColor(MaterialId materialId, string propertyName, Color value) {
            application.materialDatabase.SetInstanceProperty(element.id, materialId, propertyName, new MaterialPropertyValue2() {
                colorValue = value
            });
        }

        public void SetVector(MaterialId materialId, string propertyName, Vector4 value) {
            application.materialDatabase.SetInstanceProperty(element.id, materialId, propertyName, new MaterialPropertyValue2() {
                vectorValue = value
            });
        }

        public void SetTexture(MaterialId materialId, string propertyName, Texture texture) {
            application.materialDatabase.SetInstanceProperty(element.id, materialId, propertyName, new MaterialPropertyValue2() {
                texture = texture
            });
        }

    }

}