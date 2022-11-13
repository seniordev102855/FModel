using System;
using System.IO;
using System.Reflection;
using OpenTK.Graphics.OpenGL4;
using OpenTK.Mathematics;

namespace FModel.Views.Snooper;

public class Shader : IDisposable
{
    private readonly int _handle;

    public Shader() : this("default") {}

    public Shader(string name)
    {
        _handle = GL.CreateProgram();

        var v = LoadShader(ShaderType.VertexShader, $"{name}.vert");
        var f = LoadShader(ShaderType.FragmentShader, $"{name}.frag");
        GL.AttachShader(_handle, v);
        GL.AttachShader(_handle, f);
        GL.LinkProgram(_handle);
        GL.GetProgram(_handle, GetProgramParameterName.LinkStatus, out var status);
        if (status == 0)
        {
            throw new Exception($"Program failed to link with error: {GL.GetProgramInfoLog(_handle)}");
        }
        GL.DetachShader(_handle, v);
        GL.DetachShader(_handle, f);
        GL.DeleteShader(v);
        GL.DeleteShader(f);
    }

    private int LoadShader(ShaderType type, string file)
    {
        var executingAssembly = Assembly.GetExecutingAssembly();
        using var stream = executingAssembly.GetManifestResourceStream($"{executingAssembly.GetName().Name}.Resources.{file}");
        using var reader = new StreamReader(stream);
        var handle = GL.CreateShader(type);
        GL.ShaderSource(handle, reader.ReadToEnd());
        GL.CompileShader(handle);
        string infoLog = GL.GetShaderInfoLog(handle);
        if (!string.IsNullOrWhiteSpace(infoLog))
        {
            throw new Exception($"Error compiling shader of type {type}, failed with error {infoLog}");
        }

        return handle;
    }

    public void Use()
    {
        GL.UseProgram(_handle);
    }

    public void Render(Matrix4 viewMatrix, Vector3 viewPos, Vector3 viewDir, Matrix4 projMatrix)
    {
        Render(viewMatrix, viewPos, projMatrix);
        SetUniform("uViewDir", viewDir);
    }
    public void Render(Matrix4 viewMatrix, Vector3 viewPos, Matrix4 projMatrix)
    {
        Render(viewMatrix, projMatrix);
        SetUniform("uViewPos", viewPos);
    }
    public void Render(Matrix4 viewMatrix, Matrix4 projMatrix)
    {
        Use();
        SetUniform("uView", viewMatrix);
        SetUniform("uProjection", projMatrix);
    }

    public void SetUniform(string name, int value)
    {
        int location = GL.GetUniformLocation(_handle, name);
        ThrowIfNotFound(location, name);
        GL.Uniform1(location, value);
    }

    public unsafe void SetUniform(string name, Matrix4 value) => UniformMatrix4(name, (float*) &value);
    public unsafe void SetUniform(string name, System.Numerics.Matrix4x4 value) => UniformMatrix4(name, (float*) &value);
    public unsafe void UniformMatrix4(string name, float* value)
    {
        //A new overload has been created for setting a uniform so we can use the transform in our shader.
        int location = GL.GetUniformLocation(_handle, name);
        ThrowIfNotFound(location, name);
        GL.UniformMatrix4(location, 1, false, value);
    }

    public void SetUniform(string name, bool value) => SetUniform(name, Convert.ToUInt32(value));

    public void SetUniform(string name, uint value)
    {
        int location = GL.GetUniformLocation(_handle, name);
        ThrowIfNotFound(location, name);
        GL.Uniform1(location, value);
    }

    public void SetUniform(string name, float value)
    {
        int location = GL.GetUniformLocation(_handle, name);
        ThrowIfNotFound(location, name);
        GL.Uniform1(location, value);
    }

    public void SetUniform(string name, Vector3 value) => SetUniform3(name, value.X, value.Y, value.Z);
    public void SetUniform(string name, System.Numerics.Vector3 value) => SetUniform3(name, value.X, value.Y, value.Z);
    public void SetUniform3(string name, float x, float y, float z)
    {
        int location = GL.GetUniformLocation(_handle, name);
        ThrowIfNotFound(location, name);
        GL.Uniform3(location, x, y, z);
    }

    public void SetUniform(string name, Vector4 value) => SetUniform4(name, value.X, value.Y, value.Z, value.W);
    public void SetUniform(string name, System.Numerics.Vector4 value) => SetUniform4(name, value.X, value.Y, value.Z, value.W);
    public void SetUniform4(string name, float x, float y, float z, float w)
    {
        int location = GL.GetUniformLocation(_handle, name);
        ThrowIfNotFound(location, name);
        GL.Uniform4(location, x, y, z, w);
    }

    private void ThrowIfNotFound(int location, string name)
    {
        if (location == -1)
        {
            throw new Exception($"{name} uniform not found on shader.");
        }
    }

    public void Dispose()
    {
        GL.DeleteProgram(_handle);
    }
}
