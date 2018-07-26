using System;
using System.Collections.Concurrent;
using System.Collections.Generic;

namespace HPSocketCS
{
    public class ConnectionExtra
    {
        private ConcurrentDictionary<IntPtr, object> dict = new ConcurrentDictionary<IntPtr, object>();

        /// <summary>
        /// 获取附加数据
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        public object GetExtra(IntPtr key)
        {
            object value;
            if (dict.TryGetValue(key, out value))
            {
                return value;
            }
            return null;
        }

        /// <summary>
        /// 获取附加数据
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        public T GetExtra<T>(IntPtr key)
        {
            object value;
            if (dict.TryGetValue(key, out value))
            {
                return (T)value;
            }
            return default(T);
        }

        /// <summary>
        /// 删除附加数据
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        public bool RemoveExtra(IntPtr key)
        {
            object value;
            return dict.TryRemove(key, out value);
        }

        /// <summary>
        /// 设置附加数据
        /// </summary>
        /// <param name="key"></param>
        /// <param name="value"></param>
        /// <returns></returns>
        public bool SetExtra(IntPtr key, object newValue)
        {
            try
            {
                dict.AddOrUpdate(key, newValue, (tKey, existingVal) => { return newValue; });
                return true;
            }
            catch (OverflowException)
            {
                // 字典数目超过int.max
                return false;
            }
            catch (ArgumentNullException)
            {
                // 参数为空
                return false;
            }
            catch (Exception)
            {
                return false;
            }
        }
    }

    public class Extra<T>
    {
        private ConcurrentDictionary<IntPtr, T> dict = new ConcurrentDictionary<IntPtr, T>();

        public IntPtr[] Keys
        {
            get
            {
                ICollection<IntPtr> keys = dict.Keys;
                var keys_arr = new IntPtr[keys.Count];
                keys.CopyTo(keys_arr, 0);
                return keys_arr;
            }
        }

        /// <summary>
        /// 获取附加数据
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        public T Get(IntPtr key)
        {
            T value;
            if (dict.TryGetValue(key, out value))
            {
                return value;
            }
            return default(T);
        }

        /// <summary>
        /// 删除附加数据
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        public bool Remove(IntPtr key)
        {
            T value;
            return dict.TryRemove(key, out value);
        }

        /// <summary>
        /// 设置附加数据
        /// </summary>
        /// <param name="key"></param>
        /// <param name="value"></param>
        /// <returns></returns>
        public bool Set(IntPtr key, T newValue)
        {
            try
            {
                dict.AddOrUpdate(key, newValue, (tKey, existingVal) => { return newValue; });
                return true;
            }
            catch (OverflowException)
            {
                // 字典数目超过int.max
                return false;
            }
            catch (ArgumentNullException)
            {
                // 参数为空
                return false;
            }
            catch (Exception)
            {
                return false;
            }
        }
    }
}