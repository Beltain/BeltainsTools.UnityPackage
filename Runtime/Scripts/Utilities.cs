using System.Collections;
using System.Collections.Generic;
using System.Linq;
#if UNITY_EDITOR
using UnityEditor;
#endif
using UnityEngine;

namespace BeltainsTools.Utilities
{
    public struct Bezier
    {
        public Vector3 Point_A;
        public Vector3 Poll_A;
        public Vector3 Poll_B;
        public Vector3 Point_B;

        

        public Bezier(Vector3 Point_A, Vector3 Poll_A, Vector3 Poll_B, Vector3 Point_B)
        {
            this.Point_A = Point_A;
            this.Poll_A = Poll_A;
            this.Poll_B = Poll_B;
            this.Point_B = Point_B;
        }

        public Vector3 Evaluate(float t)
        {
            t = Mathf.Clamp01(t);
            return MathB.Vector3.CubicInterpolation(Point_A, Poll_A, Poll_B, Point_B, t);
        }
    }

    [System.Serializable]
    public class WeightedRandomGroup<T>
    {
        public List<WeightedItem<T>> Items;

        [System.Serializable]
        public class WeightedItem<T1> where T1 : T
        {
            public T1 Item;
            public int Weight;
        }

        public WeightedRandomGroup()
        {
            Items = new List<WeightedItem<T>>();
        }

        public WeightedRandomGroup(IEnumerable<T> collection, System.Func<T, int> weightCalculationFunction)
        {
            Items = new List<WeightedItem<T>>();
            foreach (T item in collection)
            {
                Items.Add(new WeightedItem<T>()
                {
                    Item = item,
                    Weight = weightCalculationFunction.Invoke(item)
                });
            }
        }

        /// <summary>Warning! Heavy on the garbage collector! Use sparingly!</summary>
        public T GetRandomItem(System.Func<T, bool> selectionFunction = null, System.Random random = null)
        {
            if (random == null)
                random = new System.Random();

            List<WeightedItem<T>> selectionList = selectionFunction == null ? Items : Items.Where(r => selectionFunction.Invoke(r.Item)).ToList();

            int sumRandomWeight = 0;
            foreach (WeightedItem<T> weightedItem in selectionList)
                sumRandomWeight += weightedItem.Weight;

            int randomRoll = random.Next(sumRandomWeight);
            foreach (WeightedItem<T> weightedItem in selectionList)
            {
                randomRoll -= weightedItem.Weight;
                if (randomRoll <= 0)
                    return weightedItem.Item;
            }
            //should never get here
            return default;
        }
    }

    public class TimeLerper
    {
        protected float m_StartValue;
        protected float m_EndValue;
        protected float m_CurrentValue;

        protected float m_DurationStart;
        protected float m_DurationRemaining;

        protected AnimationCurve m_LerpCurve;


        public bool HasEnded { get; private set; }
        public float CurrentValue => m_CurrentValue;



        public enum CurveTypes
        {
            Linear = 0,
            EaseInOut = 1,
            EaseIn = 2,
            EaseOut = 3,
        }



        public TimeLerper(float startValue, float endValue, float duration, CurveTypes curveType = CurveTypes.Linear)
        {
            Debug.Assert(duration != 0f, "Attempted to create a time lerper with a 0 duration! This is not allowed!");

            m_StartValue = startValue;
            m_CurrentValue = startValue;
            m_EndValue = endValue;
            m_DurationStart = duration;
            m_DurationRemaining = m_DurationStart;

            switch (curveType)
            {
                case CurveTypes.Linear:
                    m_LerpCurve = AnimationCurve.Linear(0f, 0f, 1f, 1f);
                    break;
                case CurveTypes.EaseInOut:
                    m_LerpCurve = AnimationCurve.EaseInOut(0f, 0f, 1f, 1f);
                    break;
                case CurveTypes.EaseIn:
                    m_LerpCurve = AnimationCurveExtensions.EaseIn(0f, 0f, 1f, 1f);
                    break;
                case CurveTypes.EaseOut:
                    m_LerpCurve = AnimationCurveExtensions.EaseOut(0f, 0f, 1f, 1f);
                    break;
            }
        }

        public static TimeLerper Create(float startValue, float endValue, float duration, CurveTypes curveType = CurveTypes.Linear) 
            => new TimeLerper(startValue, endValue, duration, curveType);

        public static TimeLerper Create_SpeedWise(float startValue, float endValue, float speed, CurveTypes curveType = CurveTypes.Linear)
            => new TimeLerper(startValue, endValue, Mathf.Abs((endValue - startValue) / speed), curveType);



        /// <summary>Update the time lerper's progress (based on delta time) and return whether it's complete or not</summary>
        public bool Update()
        {
            if (!HasEnded)
            {
                m_DurationRemaining = Mathf.Max(0f, m_DurationRemaining - Time.deltaTime);
                float progress = 1f - (m_DurationRemaining / m_DurationStart);
                progress = m_LerpCurve.Evaluate(progress); //use the value returned from the progress through the duration and slap it through a curve to retrun a smoothed (or otherwise altered) value.

                m_CurrentValue = Mathf.Lerp(m_StartValue, m_EndValue, progress);

                HasEnded = m_CurrentValue == m_EndValue;
            }

            return HasEnded;
        }
    }

    public class Parser
    {
        public static bool TryParse(string input, System.Type type, out object result)
        {
            System.ComponentModel.TypeConverter converter = System.ComponentModel.TypeDescriptor.GetConverter(type);

            if (converter.IsValid(input))
            {
                result = converter.ConvertFromString(input);
                return true;
            }
            else
            {
                result = default;
                return false;
            }
        }
    }

    public class FrameDelayedAction //Should get this to work without a specified executor (On a beltainsTools monobehaviour maybe) to avoid "can't run coroutine because object is inactive" bullshit
    {
        public static void Execute(System.Action action, MonoBehaviour executor)
        {
            executor.StartCoroutine(ExecuteActionAfterFrame(action));
        }

        static IEnumerator ExecuteActionAfterFrame(System.Action action)
        {
            yield return null;
            action.Invoke();
        }
    }

    public class ExecuteOverTime : CustomYieldInstruction
    {
        float m_TDuration;
        float m_TRemaining;
        System.Action<float> m_UpdateAction;
        TimeStepType m_TimeStepType;

        public enum TimeStepType
        {
            deltaTime,
            fixedDeltaTime,
            unscaledDeltaTime,
            fixedUnscaledDeltaTime,
        }

        /// <summary>
        /// Execute an action every specified timestep, until the duration has lapsed.
        /// <para>t is passed to the executing action, where t goes from 0-1 over the full duration</para>
        /// </summary>
        public ExecuteOverTime(float time, System.Action<float> methodToExecuteOverTime, TimeStepType timeStepType = TimeStepType.deltaTime)
        {
            m_TDuration = time == 0f ? 1f : time;
            m_TRemaining = time == 0f ? 0f : time;
            m_UpdateAction = methodToExecuteOverTime;
            if (m_UpdateAction == null)
                throw new System.Exception("Expected a method to execute over time but got null in ExecuteOverTime");
            m_TimeStepType = timeStepType;
        }

        public override bool keepWaiting
        {
            get
            {
                m_TRemaining = Mathf.Max(0f, m_TRemaining - GetTimeDelta());
                float t = 1f - (m_TRemaining / m_TDuration);
                m_UpdateAction.Invoke(t);
                return t != 1f;
            }
        }

        float GetTimeDelta()
        {
            switch (m_TimeStepType)
            {
                case TimeStepType.fixedDeltaTime:
                    return Time.fixedDeltaTime;
                case TimeStepType.unscaledDeltaTime:
                    return Time.unscaledDeltaTime;
                case TimeStepType.fixedUnscaledDeltaTime:
                    return Time.fixedUnscaledDeltaTime;
                case TimeStepType.deltaTime:
                default:
                    return Time.deltaTime;
            }
        }
    }

    public static class AutoComplete
    {
        public static IEnumerable<string> GetWordCompletionCandidateWords<T>(string partialWord, IEnumerable<T> items, System.Func<T, string> wordSelector) => GetWordCompletionCandidates(partialWord, items, wordSelector).Select(wordSelector);
        public static IEnumerable<string> GetWordCompletionCandidates(string partialWord, IEnumerable<string> items) => GetWordCompletionCandidates(partialWord, items, r => r);
        public static IEnumerable<T> GetWordCompletionCandidates<T>(string partialWord, IEnumerable<T> items, System.Func<T, string> wordSelector)
        {
            d.AssertFormat(!partialWord.Contains(' '), "Tried to get matching item for non-word {0}! Cannot autocomplete sentences!", partialWord);
            if (partialWord.IsEmpty())
                return items;

            List<T> candidateItems = new List<T>();
            foreach (T item in items)
            {
                string potentialWord = wordSelector.Invoke(item);

                int excessLength = potentialWord.Length - partialWord.Length;
                if (excessLength < 0)
                    continue;

                string partialPotentialWord = potentialWord.Substring(0, partialWord.Length);
                if (string.Compare(partialPotentialWord, partialWord, true) != 0)
                    continue;

                candidateItems.Add(item);
            }

            return candidateItems;
        }
    }

    public class Circles
    {
        public static Vector2 GetAngledPointOnUnitCircleD(float angleDegrees) => GetAngledPointOnUnitCircleD(Vector2.zero, angleDegrees);
        public static Vector2 GetAngledPointOnUnitCircleR(float angleDegrees) => GetAngledPointOnUnitCircleR(Vector2.zero, angleDegrees);
        public static Vector2 GetAngledPointOnUnitCircleD(Vector2 center, float angleDegrees) => GetAngledPointOnUnitCircleR(center, angleDegrees * Mathf.Deg2Rad);
        public static Vector2 GetAngledPointOnUnitCircleR(Vector2 center, float angleRads)
        {
            return new Vector2(
                    center.x + Mathf.Cos(angleRads),
                    center.y + Mathf.Sin(angleRads)
                );
        }
    }

    /// <summary>Get a non-random offset from a center point that appears to 'wobble' when incremented</summary>
    public class PositionWobbler
    {
        //When moving this character to their follow position, we add a little 'wobble' to make for more natural following motion.
        const int k_NumPreWobbleCircles = 3; //how many circles we use to calculate the wobble offset
        const float k_WobbleMagnitude = 0.25f; //How wobbly are we?

        float[] m_PreWobbleAngles = null; //we get some random angles which act as a seed for our wobble calculation
        float[] m_PreWobbleAngleRotationSpeeds = null; //we get some random periods which determine the speed of the wobble on the different circles

        public PositionWobbler()
        {
            m_PreWobbleAngles = new float[k_NumPreWobbleCircles];
            m_PreWobbleAngleRotationSpeeds = new float[k_NumPreWobbleCircles];

            for (int i = 0; i < k_NumPreWobbleCircles; i++)
            {
                m_PreWobbleAngles[i] = Random.Range(0f, 360f);
                m_PreWobbleAngleRotationSpeeds[i] = 360f /*Full rot*/ / /*Period*/ Random.Range(1.25f, 2.5f);
            }
        }

        public Vector3 GetOffset()
        {
            Vector2 offsetSum = Vector2.zero;
            for (int i = 0; i < m_PreWobbleAngles.Length; i++)
            {
                Vector2 wobbleCircleCenterOffset = BeltainsTools.Utilities.Circles.GetAngledPointOnUnitCircleD(((float)i / m_PreWobbleAngles.Length) * 360f) * k_WobbleMagnitude;
                offsetSum += wobbleCircleCenterOffset + BeltainsTools.Utilities.Circles.GetAngledPointOnUnitCircleD(m_PreWobbleAngles[i]) * k_WobbleMagnitude;
            }
            return (offsetSum / m_PreWobbleAngles.Length).ToVector3XZ();
        }

        public void Increment()
        {
            for (int i = 0; i < k_NumPreWobbleCircles; i++) //Increment wobble
            {
                m_PreWobbleAngles[i] += m_PreWobbleAngleRotationSpeeds[i] * Time.deltaTime;
                m_PreWobbleAngles[i] %= 360f;
            }
        }
    }

    public static class GridUtils
    {
        public struct CellEdge
        {
            public Vector2Int CellPosition;
            public Vector2 StartVertex;
            public Vector2 EndVertex;
            public Vector2 Center;
            public Vector2 Normal;


            public CellEdge(Vector2Int cellPosition, Vector2 normal, Vector2 tangent, float tileSize)
            {
                float halfCellSize = tileSize / 2f;

                CellPosition = cellPosition;

                StartVertex = CellPosition + (normal * halfCellSize) - (tangent * halfCellSize);
                EndVertex = CellPosition + (normal * halfCellSize) + (tangent * halfCellSize);

                Center = (StartVertex + EndVertex) / 2f;

                Normal = normal.normalized;
                Normal = new Vector2(Mathf.RoundToInt(Normal.x), Mathf.RoundToInt(Normal.y));
            }

            public void AlignWithNormal(Vector2 normal)
            {
                Quaternion alignmentRot = Quaternion.AngleAxis(Quaternion.Angle(Quaternion.LookRotation(Normal), Quaternion.LookRotation(normal)), Vector3.forward); //Quaternion.FromToRotation(Normal, normal);
                Vector2 newStartVertex = alignmentRot * (StartVertex - Center);
                Vector2 newEndVertex = alignmentRot * (EndVertex - Center);
                StartVertex = Center + newStartVertex;
                EndVertex = Center + newEndVertex;
                Normal = normal;
            }

            public bool NormalsAligned(Vector2 normal)
            {
                return Normal == normal || Normal == -normal;
            }

            public bool ConnectsWith(CellEdge otherEdge, bool inline)
            {
                return
                    (!inline || NormalsAligned(otherEdge.Normal)) &&
                    (StartVertex == otherEdge.EndVertex || StartVertex == otherEdge.StartVertex || EndVertex == otherEdge.StartVertex || EndVertex == otherEdge.EndVertex);
            }

            public Vector2 GetSharedVertex(CellEdge otherEdge)
            {
                Vector2 sharedVertex =
                    otherEdge.StartVertex == EndVertex ? 
                    EndVertex : 
                    otherEdge.EndVertex == StartVertex ? 
                        StartVertex : 
                        Vector2.negativeInfinity;

                d.Assert(!float.IsNegativeInfinity(sharedVertex.x));

                return sharedVertex;
            }


            public bool IsEssentiallyEqual(CellEdge otherEdge)
            {
                if (otherEdge.Equals(this))
                    return true;
                return otherEdge.StartVertex == EndVertex && otherEdge.EndVertex == StartVertex && NormalsAligned(otherEdge.Normal);
            }

            public override bool Equals(object obj)
            {
                if (obj == null || obj.GetType() != typeof(CellEdge))
                    return false;
                CellEdge other = (CellEdge)obj;
                return other.StartVertex == StartVertex && other.EndVertex == EndVertex && other.Normal == Normal;
            }

            public override int GetHashCode()
                => base.GetHashCode();
        }

        public static IEnumerable<CellEdge> GetCellEdges(IEnumerable<Vector2Int> cellPositions, float tileSize = 1f)
        {
            HashSet<Vector2Int> cellLookup = cellPositions.ToHashSet();
            Quaternion stepRotation = Quaternion.Euler(0f, 0f, 90f);
            List<CellEdge> outsideEdges = new List<CellEdge>();

            //Get all outside edges from area cells
            foreach (Vector2Int cell in cellPositions)
            {
                Vector2 normal = Vector2.up;
                Vector2 tangent = Vector2.right;
                for (int i = 0; i < 4; i++)
                {
                    CellEdge edge = new CellEdge(cell, normal, tangent, tileSize);

                    if (!cellLookup.Contains(Vector2Int.RoundToInt(cell + normal)))
                    {
                        outsideEdges.Add(edge);
                    }

                    normal = stepRotation * normal;
                    tangent = stepRotation * tangent;
                }
            }

            return outsideEdges;
        }
    }

    public static class LineUtils
    {
        public static float GetLineLength(IEnumerable<Vector3> lineVertices)
        {
            float lineLength = 0f;
            for (int i = 1; i < lineVertices.Count(); i++)
                lineLength += Vector3.Distance(lineVertices.ElementAt(i - 1), lineVertices.ElementAt(i));
            return lineLength;
        }

        /// <summary>Sample a point on a line</summary>
        public static Vector3 SampleLinePointAtDistance(IEnumerable<Vector3> lineVertices, float distanceAlongLine)
        {
            SampleLineAtDistance(lineVertices, distanceAlongLine, out Vector3 result, out Vector3 _);
            return result;
        }

        /// <summary>Sample a direction (along the line from the start vertex upwards) of the line segment at a distance along the line</summary>
        public static Vector3 SampleLineDirectionAtDistance(IEnumerable<Vector3> lineVertices, float distanceAlongLine)
        {
            SampleLineAtDistance(lineVertices, distanceAlongLine, out Vector3 _, out Vector3 result);
            return result;
        }

        /// <summary>Sample a point on a line and the direction (along the line from the start vertex upwards) of the line segment at that point</summary>
        public static void SampleLineAtDistance(IEnumerable<Vector3> lineVertices, float distanceAlongLine, out Vector3 sampledPoint, out Vector3 sampledDirection)
        {
            float distanceWalked = 0f;
            Vector3 lastVert = Vector3.negativeInfinity;
            Vector3 lastDirection = Vector3.negativeInfinity;
            foreach (Vector3 curVert in lineVertices)
            {
                if (!lastVert.IsNegativeInfinity())
                {
                    lastDirection = curVert - lastVert;
                    float distanceBetweenVerts = Vector3.Distance(lastVert, curVert);
                    if (distanceWalked + distanceBetweenVerts >= distanceAlongLine)
                    {
                        // our sample point lies within this line segment, so return it
                        sampledPoint = Vector3.Lerp(lastVert, curVert, (distanceAlongLine - distanceWalked) / distanceBetweenVerts);
                        sampledDirection = lastDirection;
                        return;
                    }
                    distanceWalked += distanceBetweenVerts;
                }
                lastVert = curVert;
            }

            // the sample distance exceeds the length of the line
            d.Assert(!lastVert.IsNegativeInfinity(), "Trying to sample point on line that contains no vertices! No!");
            sampledPoint = lastVert;
            sampledDirection = lastDirection;
            return;
        }


        public static void TrimLineFromEnd(IEnumerable<Vector3> lineVertices, float chopLengthFromEnd, out Vector3[] resultingLine)
            => SplitLine(lineVertices, GetLineLength(lineVertices) - chopLengthFromEnd, out resultingLine, out Vector3[] _);
        public static void TrimLineFromStart(IEnumerable<Vector3> lineVertices, float chopLengthFromStart, out Vector3[] resultingLine)
            => SplitLine(lineVertices, chopLengthFromStart, out Vector3[] _, out resultingLine);
        /// <summary>Split a line (collection of line verts) into 2 at the <paramref name="chopLength"/>, adding vertices at the split point</summary>
        public static void SplitLine(IEnumerable<Vector3> lineVertices, float chopLength, out Vector3[] splitLineA, out Vector3[] splitLineB)
            => SplitLine(lineVertices, chopLength, insertPreciseCutVerts: true, out splitLineA, out splitLineB);
        /// <summary>Split a line (collection of line verts) into 2 at the <paramref name="chopLength"/>, adding vertices at the split point if <paramref name="insertPreciseCutVerts"/> is true</summary>
        public static void SplitLine(IEnumerable<Vector3> lineVertices, float chopLength, bool insertPreciseCutVerts, out Vector3[] splitLineA, out Vector3[] splitLineB)
        {
            d.Assert(chopLength != 0f, "Trying to split a line at 0f along it, this operation is not allowed! You need to chop it somewhere along the line...");

            int originalLineVertCount = lineVertices.Count();

            float walkedLength = 0f;
            int walkedLineVertsCount = 0;
            Vector3 cutVert = Vector3.negativeInfinity;
            bool hasCutVert = false;

            //go through all verts and cut when we've walked far enough
            Vector3 lastVert = Vector3.negativeInfinity;
            foreach (Vector3 currentVert in lineVertices)
            {

                if (!lastVert.IsNegativeInfinity())
                {
                    // == per edge calculations ==
                    float lineEdgeLength = Vector3.Distance(lastVert, currentVert);

                    if (walkedLength + lineEdgeLength >= chopLength)
                    {
                        // = Do chop =
                        if (insertPreciseCutVerts)
                        {
                            hasCutVert = true;
                            cutVert = Vector3.Lerp(lastVert, currentVert, (chopLength - walkedLength) / lineEdgeLength);
                        }
                        break;
                    }

                    walkedLength += lineEdgeLength;
                }

                walkedLineVertsCount++;
                lastVert = currentVert;
            }

            // assign verts to the resulting split arrays
            splitLineA = new Vector3[walkedLineVertsCount + (hasCutVert ? 1 : 0)];
            splitLineB = new Vector3[originalLineVertCount - walkedLineVertsCount + (hasCutVert ? 1 : 0)];

            for (int i = 0; i < walkedLineVertsCount; i++)
            {
                splitLineA[i] = lineVertices.ElementAt(i);
            }
            if (hasCutVert)
            {
                splitLineA[splitLineA.Length - 1] = cutVert;
                splitLineB[0] = cutVert;
            }
            for (int i = (hasCutVert ? 1 : 0); i < splitLineB.Length; i++)
            {
                splitLineB[i] = lineVertices.ElementAt(walkedLineVertsCount + i - (hasCutVert ? 1 : 0));
            }
        }
    }

    public static class AssetDatabaseUtils
    {
#if UNITY_EDITOR
        public static IEnumerable<T> GetPrefabsOfTypeInFolder<T>(string path) where T : Component
            => GetPrefabsOfTypeInFolder(path, typeof(T)).Select(r => r.GetComponent<T>());
        public static IEnumerable<Component> GetPrefabsOfTypeInFolder(string path, System.Type type)
        {
            string[] gUIDs = AssetDatabase.FindAssets("t:prefab", new string[] { path });
            IEnumerable<string> objectsPaths = gUIDs.Select(r => AssetDatabase.GUIDToAssetPath(r));
            IEnumerable<Component> components = objectsPaths.Select(r => AssetDatabase.LoadAssetAtPath<Component>(r));
            components = components.Where(r => r.GetComponent(type) != null);
            return components.Select(r => r.GetComponent(type));
        }

        public static IEnumerable<T> GetAllAssetsOfType<T>() where T : Object
        {
            string[] gUIDs = AssetDatabase.FindAssets($"t:{typeof(T).Name}");
            IEnumerable<string> objectsPaths = gUIDs.Select(r => AssetDatabase.GUIDToAssetPath(r));
            return objectsPaths.Select(r => AssetDatabase.LoadAssetAtPath<T>(r));
        }


        public static void EmptyFolderObjects(string folderPath)
        {
            if (!folderPath.EndsWith('/'))
                folderPath += '/';

            if (!AssetDatabase.IsValidFolder(folderPath))
                return; //no folder to empty

            string[] assetsGUIDsInFolder = AssetDatabase.FindAssets("", new string[] { folderPath });
            foreach (string assetGUID in assetsGUIDsInFolder)
            {
                string assetPath = AssetDatabase.GUIDToAssetPath(assetGUID);

                if (AssetDatabase.IsValidFolder(assetPath))
                    continue; //we dont clear folders or their contents if they're not the given folder

                AssetDatabase.DeleteAsset(assetPath);
            }

            AssetDatabase.Refresh();
        }

        public static Sprite CreateSpriteInFolder(Sprite sprite, string folderPath)
        {
            d.Assert(folderPath.StartsWith("Assets/"));

            string fullTexturePath = folderPath + sprite.name + ".png";
            string fullSpriteAssetPath = folderPath + sprite.name + ".asset";
            string directoryPath = System.IO.Path.GetDirectoryName(fullTexturePath);

            d.Assert(System.IO.Directory.Exists(directoryPath), $"No valid directory at {directoryPath} ({folderPath})! Cannot save sprite!");

            //Save and reload texture
            Texture2D texture = sprite.texture;
            byte[] textureBytes = texture.EncodeToPNG();
            System.IO.File.WriteAllBytes(fullTexturePath, textureBytes);
            AssetDatabase.Refresh();
            Texture2D savedTexture = AssetDatabase.LoadAssetAtPath<Texture2D>(fullTexturePath);
            //

            TextureImporter textureImporter = AssetImporter.GetAtPath(fullTexturePath) as TextureImporter;
            textureImporter.textureType = TextureImporterType.Sprite;
            textureImporter.spriteImportMode = SpriteImportMode.Single;
            textureImporter.SaveAndReimport();
            AssetDatabase.SaveAssets();

            foreach (Object loadedAsset in AssetDatabase.LoadAllAssetsAtPath(fullTexturePath))
            {
                if(loadedAsset is Sprite)
                    return loadedAsset as Sprite;
            }

            //Sprite savedSprite = Sprite.Create(savedTexture, sprite.rect, new Vector2(sprite.pivot.x / sprite.rect.width, sprite.pivot.y / sprite.rect.height));
            //AssetDatabase.CreateAsset(savedSprite, fullSpriteAssetPath);
            //AssetDatabase.SaveAssets();

            d.LogError($"Something went wrong when trying to create sprite {sprite.name} at path {fullTexturePath}");

            return null;//AssetDatabase.LoadAssetAtPath<Sprite>(fullSpriteAssetPath);
        }
#endif
    }

    public static class RectTransformUtils
    {
        /// <summary>Convert the <paramref name="screenPoint"/> to a point on the <paramref name="rectTransform"/> 
        /// representing the percentage X and Y that the point falls at.</summary>
        /// <returns>Whether we've hit the rect at all or not.</returns>
        /// <param name="camera">From which camera are we sampling? For points on a screenspace overlay canvas, leave null.</param>
        public static bool ScreenPointToRectXY01(RectTransform rectTransform, Vector2 screenPoint, out Vector2 xy01, Camera camera = null)
        {
            bool hit = ScreenPointToRectUV(rectTransform, screenPoint, out xy01, camera);
            xy01 = xy01.SetY(1f - xy01.y); // flip to rectTransfrom XY space
            return hit;
        }

        /// <summary>Convert the <paramref name="screenPoint"/> to a UV point on the <paramref name="rectTransform"/></summary>
        /// <returns>Whether we've hit the rect at all or not.</returns>
        /// <param name="camera">From which camera are we sampling? For points on a screenspace overlay canvas, leave null.</param>
        public static bool ScreenPointToRectUV(RectTransform rectTransform, Vector2 screenPoint, out Vector2 uvPoint, Camera camera = null)
        {
            uvPoint = Vector2.zero;
            if (RectTransformUtility.ScreenPointToLocalPointInRectangle(rectTransform,
                screenPoint, camera, out Vector2 localPointHit))
            {
                // Convert localPoint to normalized UV coordinates
                uvPoint.x = (localPointHit.x + (rectTransform.rect.width * 0.5f)) / rectTransform.rect.width;
                uvPoint.y = 1f - ((localPointHit.y + (rectTransform.rect.height * 0.5f)) / rectTransform.rect.height); // flip to UV space

                return new Rect(Vector2.zero, Vector2.one).Contains(uvPoint); // return whether we're in UV bounds 0 -> 1
            }
            else
            {
                return false;
            }
        }
    }
}

