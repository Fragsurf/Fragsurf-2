using UnityEngine;
using Steamworks;
using UnityEngine.UI;

public class SteamAvatar : MonoBehaviour
{
    public ulong SteamId;
    public Texture FallbackTexture;

    void Start()
    {
        Fetch();
    }

    public async void Fetch()
    {
        if (SteamId == 0 || !SteamClient.IsValid)
        {
            SetTexture(FallbackTexture);
            return;
        }

        var image = await SteamFriends.GetSmallAvatarAsync(SteamId);
        if (!image.HasValue)
        {
            SetTexture(FallbackTexture);
            return;
        }
        var texture = new Texture2D((int)image.Value.Width, (int)image.Value.Height);

        for (int x = 0; x < image.Value.Width; x++)
        for (int y = 0; y < image.Value.Height; y++)
        {
            var p = image.Value.GetPixel(x, y);
            texture.SetPixel(x, (int)image.Value.Height - y, new Color(p.r / 255.0f, p.g / 255.0f, p.b / 255.0f, p.a / 255.0f));
        }

        texture.Apply();
        SetTexture(texture);
    }

    private void SetTexture(Texture texture)
    {
        var rawImage = GetComponent<RawImage>();
        if (rawImage != null)
            rawImage.texture = texture;
    }
}