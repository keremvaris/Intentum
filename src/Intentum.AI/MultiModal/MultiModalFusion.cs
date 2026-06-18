namespace Intentum.AI.MultiModal;

public sealed class MultiModalFusion
{
    public float[] Fuse(float[] behaviorEmbedding, IReadOnlyList<MultiModalInput> additionalInputs)
    {
        ArgumentNullException.ThrowIfNull(behaviorEmbedding);

        if (additionalInputs.Count == 0)
            return behaviorEmbedding;

        var modalityEmbeddings = additionalInputs
            .Where(i => i.Embedding != null)
            .GroupBy(i => i.Modality)
            .Select(g =>
            {
                var embeddings = g.Select(i => i.Embedding!).ToList();
                var avg = new float[embeddings[0].Length];
                for (int j = 0; j < avg.Length; j++)
                    avg[j] = embeddings.Average(e => e[j]);
                return avg;
            })
            .ToList();

        var totalLength = behaviorEmbedding.Length + modalityEmbeddings.Sum(e => e.Length);
        var result = new float[totalLength];
        behaviorEmbedding.CopyTo(result, 0);

        var offset = behaviorEmbedding.Length;
        foreach (var emb in modalityEmbeddings)
        {
            emb.CopyTo(result, offset);
            offset += emb.Length;
        }

        return result;
    }
}
