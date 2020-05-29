using System;
using System.Collections.Generic;
using System.Numerics;
using System.Text;

namespace _3Dtest
{
    abstract class Light
    {
        public Vector3 Ambient = new Vector3(0.05f, 0.05f, 0.05f);
        public Vector3 Diffuse = new Vector3(0.8f, 0.8f, 0.8f);
        public Vector3 Specular = new Vector3(1.0f, 1.0f, 1.0f);

        public Light() { }

        public Light(Vector3 ambient, Vector3 diffuse, Vector3 specular)
        {
            Ambient = ambient;
            Diffuse = diffuse;
            Specular = specular;
        }

        public abstract void SetValues(Shader shader, int index);
        protected virtual void SetColorValues(Shader shader, string name)
        {
            shader.SetVec3(name + ".color.ambient", Ambient);
            shader.SetVec3(name + ".color.diffuse", Diffuse);
            shader.SetVec3(name + ".color.specular", Specular);
        }
    }

    class DirectionalLight : Light
    {
        public Vector3 Direction;

        public DirectionalLight(Vector3 direction) : base()
        {
            Direction = direction;
        }

        public DirectionalLight(Vector3 direction, Vector3 ambient, Vector3 diffuse, Vector3 specular) : base(ambient, diffuse, specular)
        {
            Direction = direction;
        }

        public override void SetValues(Shader shader, int index)
        {
            string name = "dirLights[" + index + "]";

            base.SetColorValues(shader, name);

            shader.SetVec3(name + ".direction", Direction);
        }
    }

    class PointLight : Light
    {
        public Vector3 Position;
        public float Constant = 1, Linear = .09f, Quadratic = .032f;

        public PointLight(Vector3 position) : base()
        {
            Position = position;
        }
        public PointLight(Vector3 position, float constant, float linear, float quadratic) : base()
        {
            Position = position;

            Constant = constant;
            Linear = linear;
            Quadratic = quadratic;
        }

        public PointLight(Vector3 position, Vector3 ambient, Vector3 diffuse, Vector3 specular) : base(ambient, diffuse, specular)
        {
            Position = position;
        }
        public PointLight(Vector3 position, float constant, float linear, float quadratic, Vector3 ambient, Vector3 diffuse, Vector3 specular) : base(ambient, diffuse, specular)
        {
            Position = position;

            Constant = constant;
            Linear = linear;
            Quadratic = quadratic;
        }

        public override void SetValues(Shader shader, int index)
        {
            string name = "pointLights[" + index + "]";

            base.SetColorValues(shader, name);

            shader.SetVec3(name + ".position", Position);
            shader.SetFloat(name + ".constant", Constant);
            shader.SetFloat(name + ".linear", Linear);
            shader.SetFloat(name + ".quadratic", Quadratic);
        }
    }

    class SpotLight : Light
    {
        public Vector3 Position;
        public Vector3 Direction = new Vector3(0, 0, 1);
        public float Cutoff = .5f, OuterCutoff = .6f;
        public float Constant = 1, Linear = 1, Quadratic = 1;

        public SpotLight(Vector3 position, float cutoff, float outerCutoff) : this(position, cutoff, outerCutoff, 1, 0.09f, 0.032f) { }
       
        public SpotLight(Vector3 position, float cutoff, float outerCutoff, float constant, float linear, float quadratic) : base()
        {
            Position = position;

            Cutoff = cutoff;
            OuterCutoff = outerCutoff;

            Constant = constant;
            Linear = linear;
            Quadratic = quadratic;
        }

        public SpotLight(Vector3 position, float cutoff, float outerCutoff, Vector3 ambient, Vector3 diffuse, Vector3 specular) : this(position, cutoff, outerCutoff, 1, 0.09f, 0.032f, ambient, diffuse, specular) { }
        
        public SpotLight(Vector3 position, float cutoff, float outerCutoff, float constant, float linear, float quadratic, Vector3 ambient, Vector3 diffuse, Vector3 specular) : base(ambient, diffuse, specular)
        {
            Position = position;

            Cutoff = cutoff;
            OuterCutoff = outerCutoff;

            Constant = constant;
            Linear = linear;
            Quadratic = quadratic;
        }

        public override void SetValues(Shader shader, int index)
        {
            string name = "spotLights[" + index + "]";

            base.SetColorValues(shader, name);

            shader.SetVec3(name + ".position", Position);
            shader.SetVec3(name + ".direction", Direction);
            shader.SetFloat(name + ".cutoff", Cutoff);
            shader.SetFloat(name + ".outerCutoff", OuterCutoff);
            shader.SetFloat(name + ".constant", Constant);
            shader.SetFloat(name + ".linear", Linear);
            shader.SetFloat(name + ".quadratic", Quadratic);
        }
    }
}
