using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Text;

namespace Assimp.Unmanaged
{
    /// <summary>
    /// 手动处理 UTF-8 字符串的封送
    /// </summary>
    public class Utf8StringMarshaler : ICustomMarshaler
    {
        // 单例模式，CLR 会通过 GetInstance 获取实例
        private static readonly Utf8StringMarshaler Instance = new Utf8StringMarshaler();

        /// <summary>
        /// 获取实例
        /// </summary>
        public static ICustomMarshaler GetInstance(string cookie)
        {
            return Instance;
        }

        /// <summary>
        /// 将托管字符串转换为非托管 UTF-8 内存指针
        /// </summary>
        public IntPtr MarshalManagedToNative(object managedObj)
        {
            if (managedObj == null)
                return IntPtr.Zero;

            if (managedObj is not string str)
                throw new MarshalDirectiveException("Utf8StringMarshaler must be used on a string.");

            // 计算 UTF-8 字节长度
            int byteCount = Encoding.UTF8.GetByteCount(str);

            // 分配非托管内存：字节数 + 1 (用于 null 终止符 '\0')
            IntPtr nativePtr = Marshal.AllocHGlobal(byteCount + 1);

            if (byteCount > 0)
            {
                unsafe
                {
                    byte* bytes = (byte*)nativePtr.ToPointer();
                    fixed (char* chars = str)
                    {
                        // 将字符串编码为 UTF-8 并写入非托管内存
                        Encoding.UTF8.GetBytes(chars, str.Length, bytes, byteCount);
                    }
                    // 添加 null 终止符
                    bytes[byteCount] = 0;
                }
            }
            else
            {
                // 空字符串的情况，只写入 null 终止符
                Marshal.WriteByte(nativePtr, 0, 0);
            }

            return nativePtr;
        }

        /// <summary>
        /// 清理非托管内存
        /// </summary>
        public void CleanUpNativeData(IntPtr pNativeData)
        {
            if (pNativeData != IntPtr.Zero)
            {
                Marshal.FreeHGlobal(pNativeData);
            }
        }

        /// <summary>
        /// 将非托管 UTF-8 内存指针转换回托管字符串
        /// 注意：对于 [In] 参数，此方法通常不会被调用，除非是 [Out] 或返回值
        /// </summary>
        public object MarshalNativeToManaged(IntPtr pNativeData)
        {
            if (pNativeData == IntPtr.Zero)
                return null;

            // 手动扫描到 \0 再解码。
            unsafe
            {
                byte* p = (byte*)pNativeData;
                int len = 0;
                while (p[len] != 0) len++;
                return Encoding.UTF8.GetString(p, len);
            }
        }

        /// <summary>
        /// CLR（公共语言运行时）会在封送处理结束且托管数据不再被引用时自动调用此方法
        /// </summary>
        public void CleanUpManagedData(object managedObj)
        {
            // 托管对象由 GC 管理，无需操作
        }

        /// <summary>
        /// 要封送的非托管数据的大小
        /// </summary>
        public int GetNativeDataSize()
        {
            return -1; // 表示大小不固定
        }
    }
}
