
public class VoronoiBiomes : VoronoiMono<Biome>
{
    public static VoronoiBiomes inst;

    public override void Awake()
    {
        inst = this;
        base.Awake();
    }
}

