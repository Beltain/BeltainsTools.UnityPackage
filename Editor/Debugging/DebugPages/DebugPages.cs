using BeltainsTools.BTInternal;
using BeltainsTools.EventHandling;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEditor;
using UnityEngine;

namespace BeltainsTools.Editor.Debugging.DebugPages
{
    /// <summary>
    /// Implement this on any class (with a parameterless constructor) to add a new page
    /// to the debug pages window. No manual registration needed — pages are found via reflection.
    /// </summary>
    public interface IDebugPage
    {
        void OnGUI();
        void OnEnable();
        void OnDisable();
    }

    /// <summary>
    /// Optional base class so new pages don't have to implement every interface member.
    /// Most new pages should inherit from this instead of implementing <see cref="IDebugPage"/> directly.
    /// </summary>
    public abstract class DebugPage : IDebugPage
    {
        public abstract void OnGUI();
        public virtual void OnEnable() { }
        public virtual void OnDisable() { }
    }

    /// <summary>
    /// Draws the exact default inspector for a <see cref="UnityEngine.Object"/>s 
    /// inside the debug window, same as you'd see in the normal Inspector tab.
    /// </summary>
    public class ObjectInspectorDebugPage : DebugPage
    {
        private UnityEngine.Object m_Target;
        private UnityEditor.Editor m_CachedEditor;

        public UnityEngine.Object Target => m_Target;

        public BEvent<UnityEngine.Object> TargetChangedEvent;

        public ObjectInspectorDebugPage(UnityEngine.Object target)
        {
            m_Target = target;
        }

        public override void OnGUI()
        {
            EditorGUI.BeginChangeCheck();
            m_Target = EditorGUILayout.ObjectField(
                "Target", m_Target, typeof(UnityEngine.Object), true);
            if (EditorGUI.EndChangeCheck())
                TargetChangedEvent.Invoke(m_Target);

            if (m_Target == null)
            {
                m_CachedEditor = null;
                return;
            }

            EditorGUI.BeginChangeCheck();

            UnityEditor.Editor.CreateCachedEditor(m_Target, null, ref m_CachedEditor);
            m_CachedEditor.OnInspectorGUI();

            if (EditorGUI.EndChangeCheck())
            {
                Undo.RecordObject(m_Target, "Modify " + m_Target.name);
                EditorUtility.SetDirty(m_Target);
            }
        }

        public override void OnDisable()
        {
            if (m_CachedEditor != null)
                Object.DestroyImmediate(m_CachedEditor);
        }
    }


    [System.AttributeUsage(System.AttributeTargets.Class)]
    public class DebugPageAttribute : System.Attribute
    {
        public string PageName { get; }
        public int TabOrder { get; }

        public DebugPageAttribute(string tabName, int tabOrder = 0)
        {
            PageName = tabName;
            TabOrder = tabOrder;
        }
    }


    public class DebugPages : EditorWindow
    {
        public const string k_WindowTitle = "Debug Pages";
        public const string k_TabTitle_Singular = "Page";
        public const string k_TabTitle_Plural = k_TabTitle_Singular + "s";

        private DebugPagesSessionData m_SessionData;
        private List<PageEntry> m_Pages = new List<PageEntry>();
        private int m_SelectedIndex;
        private Vector2 m_PageTabsScrollPos;
        private Vector2 m_ContentScrollPos;

        private DebugPagesSessionData Session
        {
            get
            {
                if (m_SessionData == null)
                    m_SessionData = IO.GetOrCreateEditorSessionDataObject(typeof(DebugPagesSessionData), "_DebugPagesData.asset") as DebugPagesSessionData;
                return m_SessionData;
            }
        }

        [MenuItem(PackageData.Paths.MenuItem.k_Window + k_WindowTitle)]
        public static void ShowWindow() 
        {
            DebugPages window = GetWindow<DebugPages>(k_WindowTitle);
            window.minSize = new Vector2(500, 300);
        }

        private abstract class PageEntry : System.IDisposable
        {
            public IDebugPage Page { get; protected set; } = null;

            public void Dispose()
            {
                Page.OnDisable();
                Page = null;
            }

            public abstract string GetName();
            public abstract int GetOrder();
        }

        private class PageEntry_Object : PageEntry
        {
            UnityEngine.Object m_TargetObject;
            int m_Index;

            public int Index => m_Index;

            private System.Action<PageEntry_Object, UnityEngine.Object> m_TargetChangedCallback;

            public PageEntry_Object(UnityEngine.Object targetObj, int index, System.Action<PageEntry_Object, UnityEngine.Object> targetChangedCallback)
            {
                m_TargetObject = targetObj;
                m_Index = index;
                TryChangeTargetObject(targetObj, true);
                m_TargetChangedCallback = targetChangedCallback;
            }

            public bool TryChangeTargetObject(UnityEngine.Object newTarget, bool force = false)
            {
                if (m_TargetObject == newTarget && !force)
                    return false;

                if (Page != null)
                {
                    Page.OnDisable();
                    ((ObjectInspectorDebugPage)Page).TargetChangedEvent.Unsubscribe(OnTargetObjectChanged);
                }

                m_TargetObject = newTarget;
                Page = new ObjectInspectorDebugPage(newTarget);

                ((ObjectInspectorDebugPage)Page).TargetChangedEvent.Subscribe(OnTargetObjectChanged);
                Page.OnEnable();

                m_TargetChangedCallback?.Invoke(this, m_TargetObject);

                return true;
            }

            public override string GetName()
            {
                string name = m_TargetObject != null ? m_TargetObject.name : "new";
                return $"[ {name} ]";
            }

            public override int GetOrder() => m_Index;

            private void OnTargetObjectChanged(UnityEngine.Object newTarget)
            {
                TryChangeTargetObject(newTarget);
            }
        }

        private class PageEntry_Type : PageEntry
        {
            public DebugPageAttribute Attribute;

            public PageEntry_Type(System.Type type)
            {
                Page = (IDebugPage)System.Activator.CreateInstance(type);
                Page.OnEnable();
                Attribute = type.GetCustomAttribute<DebugPageAttribute>();
            }

            public override string GetName() => Attribute.PageName;
            public override int GetOrder() => Attribute.TabOrder;
        }


        private void OnEnable()
        {
            DiscoverPages();
        }

        private void OnDisable()
        {
            ClearPages();
        }

        private void DiscoverPages()
        {
            ClearPages();

            DiscoverTypePages(m_Pages);
            List<PageEntry> objectPages = new List<PageEntry>();
            DiscoverObjectPages(objectPages);
            m_Pages.AddRange(objectPages);
            m_SelectedIndex = Mathf.Clamp(m_SelectedIndex, 0, Mathf.Max(0, m_Pages.Count - 1));
        }

        private void ClearPages()
        {
            foreach (PageEntry pageEntry in m_Pages)
                pageEntry.Dispose();
            m_Pages.Clear();
        }

        private void DiscoverTypePages(List<PageEntry> pages)
        {
            foreach (Assembly assembly in System.AppDomain.CurrentDomain.GetAssemblies())
            {
                foreach (System.Type type in SafeGetTypes(assembly))
                {
                    if (!typeof(IDebugPage).IsAssignableFrom(type) || type.IsInterface || type.IsAbstract
                        || type.GetConstructor(System.Type.EmptyTypes) == null)
                        continue;
                    DebugPageAttribute attribute = type.GetCustomAttribute<DebugPageAttribute>();
                    if (attribute == null)
                        continue;
                    pages.Add(new PageEntry_Type(type));
                }
            }
            SortPages(pages);
        }

        private void DiscoverObjectPages(List<PageEntry> pages)
        {
            for (int i = 0; i < Session.TrackedObjects.Count; i++)
            {
                PageEntry_Object pageEntry = new PageEntry_Object(Session.TrackedObjects[i], i, (PageEntry_Object entry, UnityEngine.Object newTarget) =>
                {
                    Session.TrackedObjects[entry.Index] = newTarget;
                    EditorUtility.SetDirty(Session);
                });
                pages.Add(pageEntry);
            }

            SortPages(pages);
        }

        private void SortPages(List<PageEntry> pages)
        {
            pages.Sort((a, b) =>
            {
                int orderComparison = a.GetOrder().CompareTo(b.GetOrder());
                if (orderComparison != 0)
                    return orderComparison;
                return string.Compare(a.GetName(), b.GetName(), System.StringComparison.Ordinal);
            });
        }




        private static IEnumerable<System.Type> SafeGetTypes(Assembly assembly)
        {
            try { return assembly.GetTypes(); }
            catch (ReflectionTypeLoadException e) { return e.Types.Where(t => t != null); }
        }

        private void OnGUI()
        {
            // refresh object page targets incase they've been changed in our session data
            IEnumerable<PageEntry_Object> objectPages = m_Pages.Where(r => r is PageEntry_Object).Cast<PageEntry_Object>();
            if (objectPages.Count() != Session.TrackedObjects.Count)
            {
                DiscoverPages();
                return;
            }
            else
            {
                foreach (PageEntry_Object objectPage in objectPages)
                    objectPage.TryChangeTargetObject(Session.TrackedObjects[objectPage.Index]);
            }

            if (m_Pages.Count > 0)
            {
                EditorGUI.BeginChangeCheck();
                EditorGUILayout.BeginHorizontal();
                m_PageTabsScrollPos = EditorGUILayout.BeginScrollView(m_PageTabsScrollPos, GUILayout.Height(36f));
                int newIndex = GUILayout.Toolbar(m_SelectedIndex, m_Pages.Select(t => t.GetName()).ToArray());
                EditorGUILayout.EndScrollView();
                if (EditorGUI.EndChangeCheck())
                    m_SelectedIndex = newIndex;

                // allow for adding and removing tracked objects pages
                EditorGUI.BeginChangeCheck();
                if (GUILayout.Button("+", GUILayout.Width(20)))
                {
                    Session.TrackedObjects.Add(null);
                    DiscoverPages();
                }

                EditorGUI.BeginDisabledGroup(!(m_Pages[m_SelectedIndex] is PageEntry_Object)); // THIS WILL BREAK IF WE EVER HAVE MORE THAN TWO PAGE ENTRY TYPES OR WE CHANGE THE ORDER
                if (GUILayout.Button("-", GUILayout.Width(20)))
                {
                    Session.TrackedObjects.RemoveAt(m_SelectedIndex - m_Pages.Count(t => t is PageEntry_Type)); // THIS WILL BREAK IF WE EVER HAVE MORE THAN TWO PAGE ENTRY TYPES OR WE CHANGE THE ORDER
                    m_SelectedIndex--;
                    DiscoverPages();
                }
                EditorGUI.EndDisabledGroup();
                if (EditorGUI.EndChangeCheck())
                    EditorUtility.SetDirty(Session);
                EditorGUILayout.EndHorizontal();
            }

            if (m_Pages.Count == 0)
            {
                EditorGUILayout.HelpBox($"No {k_TabTitle_Plural} found!\n" +
                    $"Implement at least one class with the {nameof(IDebugPage)} interface or {nameof(DebugPage)} base class and decorate it with a [{nameof(DebugPageAttribute)}],\n" +
                    $"or, add a tracked object:", MessageType.Warning);

                if (GUILayout.Button("Add an Object to Track"))
                {
                    Session.TrackedObjects.Add(null);
                    DiscoverPages();
                }
                return;
            }

            EditorGUILayout.Space(6);
            m_ContentScrollPos = EditorGUILayout.BeginScrollView(m_ContentScrollPos);
            m_Pages[m_SelectedIndex].Page.OnGUI();
            EditorGUILayout.EndScrollView();
        }

    }
}
