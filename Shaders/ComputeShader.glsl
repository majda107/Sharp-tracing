#version 430

#define M_PI 3.14159

layout(local_size_x = 1, local_size_y = 1) in;
layout(rgba32f, binding = 0) uniform image2D img_output;
layout(binding = 1) uniform samplerCube cubeMap;

uniform vec4 spheres[6];

//uniform vec4 sphere;
//uniform vec4 sphere2;
uniform mat4 cameraViewMatrix;

bool sphere_intersect(vec4 sphere, vec3 ray_origin, vec3 direction)
{
	vec3 oc = ray_origin - vec3(sphere);
	float a = dot(direction, direction);
	float b = 2.0 * dot(oc, direction);
	float c = dot(oc, oc) - sphere.w * sphere.w;
	float discriminant = b*b - 4*a*c;
	return discriminant > 0;
}

vec3 sphere_point_intersect(vec4 sphere, vec3 ray_origin, vec3 direction)
{
	vec3 oc = ray_origin - sphere.xyz;
	float a = dot(direction, direction);
	float b = 2.0 * dot(oc, direction);
	float c = dot(oc, oc) - sphere.w * sphere.w;
	float discriminant = b*b - 4*a*c;
    if(discriminant < 0){
        return ray_origin;
    }
    else{
        float t = (-b - sqrt(discriminant)) / (2.0*a);
		return ray_origin + t * direction;
    }
}

float sphere_intersect_distance(vec4 sphere, vec3 ray_origin, vec3 direction)
{
	vec3 oc = ray_origin - sphere.xyz;
	float a = dot(direction, direction);
	float b = 2.0 * dot(oc, direction);
	float c = dot(oc, oc) - sphere.w * sphere.w;
	float discriminant = b*b - 4*a*c;
    if(discriminant < 0){
        return -1;
    }
    else{
        return (-b - sqrt(discriminant)) / (2.0*a);
    }
}

vec3 plane_point_intersect(vec3 plane_center, vec3 plane_normal, vec3 ray_origin, vec3 ray_direction)
{
	float d = dot(-plane_normal, ray_direction);
	if(d < 0.0001) return ray_origin;

	float t = -dot((plane_center - ray_origin), plane_normal) / d;

	return ray_origin + t * ray_direction;
}

void main() 
{
	vec4 sphere = spheres[0];
	vec4 sphere2 = spheres[1];

	vec4 pixel = vec4(1.0, 1.0, 1.0, 1.0);
	ivec2 pixel_coords = ivec2(gl_GlobalInvocationID.xy);

	float max_x = 10.0;
	float max_y = 10.0;
	ivec2 dims = imageSize(img_output); // fetch image dimensions
	float x = (float(pixel_coords.x * 2 - dims.x) / dims.x);  // -1 to 1
	float y = (float(pixel_coords.y * 2 - dims.y) / dims.y);  // -1 to 1

	float fov = 45;
	float aspectRatio = max_x / max_y;
	float Px = (x) * tan(fov / 2 * M_PI / 180) * aspectRatio; 
	float Py = (y) * tan(fov / 2 * M_PI / 180);
	
	vec3 ray_o = vec3(0.0, 0.0, 0.0); 
//	vec3 ray_d = vec3(Px, Py, 1.0) - ray_o; 
//	ray_d = normalize(ray_d);

	vec3 ray_d = vec3(Px, Py, -1.0);

	vec3 ray_o_world;
	vec3 ray_p_world;

	ray_o_world = (cameraViewMatrix * vec4(ray_o, 1.0)).xyz;

	ray_p_world = (cameraViewMatrix * vec4(ray_d, 1.0)).xyz;
	ray_p_world = ray_p_world - ray_o_world;
	ray_p_world = normalize(ray_p_world);

	vec3 plane_center = vec3(0, 10.0, 0);
	vec3 plane_normal = normalize(vec3(0, -1.0f, 0));

	//vec3 light_source = vec3(-5.0, -5.0, 0);
	vec3 light_source = vec3(20.0, -20.0, 8.0);
	//vec3 light_source = vec3(0.0, 0.0, 0.0);

	for(int repeat = 0; repeat < 8; repeat++)
	{
		float sphere_distance = -1;
		int hit_index = -1;
		for(int i = 0; i < 6; i++)
		{
			float this_sphere_distance = sphere_intersect_distance(spheres[i], ray_o_world, ray_p_world);
			if(this_sphere_distance == -1) continue;
			if(this_sphere_distance < sphere_distance || sphere_distance == -1)
			{
				sphere_distance = this_sphere_distance;
				hit_index = i;
			}
		}

		if(sphere_distance > -1)
		{
			ray_o_world = ray_o_world + sphere_distance * ray_p_world;
			vec3 sphere_surface_normal = normalize(ray_o_world - spheres[hit_index].xyz);
			ray_p_world = reflect(ray_p_world, sphere_surface_normal);
			pixel = mix(pixel, vec4(0.1, 0.1, 0.1, 1.0), 0.25);
		}
		else
		{
			vec3 plane_intersection_point = plane_point_intersect(plane_center, plane_normal, ray_o_world, ray_p_world);
			if(plane_intersection_point == ray_o_world) // IF SKY WAS HIT
			{
				vec4 color = texture(cubeMap, normalize(vec3(ray_p_world.x, -ray_p_world.y, ray_p_world.z)));
				pixel = mix(color, pixel, 0.1);
				break;
			}
			else
			{
				ray_o_world = plane_intersection_point;
				ray_p_world = reflect(ray_p_world, plane_normal);

				pixel = mix(vec4(0.3, 0.3, 0.3, 1.0), pixel, 0.5);

				vec3 to_light_direction = normalize(light_source - ray_o_world);
				for(int i = 0; i < 6; i++)
				{	
					vec3 sphere_intersection_point = sphere_point_intersect(spheres[i], ray_o_world, to_light_direction);
					if(sphere_intersection_point != ray_o_world)
					{
						pixel = mix(vec4(0.0, 0.0, 0.0, 1.0), pixel, 0.01);
						break;
					}
				}
			}
		}
	}
	

	//ALMOST FUNCTIONAL
//	vec3 sphere_intersection_point = sphere_point_intersect(sphere, ray_o_world, ray_p_world);
//	vec3 sphere2_intersection_point = sphere_point_intersect(sphere2, ray_o_world, ray_p_world);
//
//	if(sphere_intersection_point != ray_o_world)
//	{
//		vec3 sphere_surface_normal = normalize(sphere_intersection_point - sphere.xyz);
//		ray_p_world = reflect(ray_p_world, sphere_surface_normal);
//
//		vec3 plane_intersection_point = plane_point_intersect(plane_center, plane_normal, sphere_intersection_point, ray_p_world);
//		if(plane_intersection_point == sphere_intersection_point)
//		{
//			pixel = texture(cubeMap, normalize(vec3(ray_p_world.x, -ray_p_world.y, ray_p_world.z)));
//			pixel = mix(pixel, vec4(0.0, 0.0, 0.0, 1.0), 0.25);
//		}
//		else
//		{
//			vec3 plane_intersection_point = plane_point_intersect(plane_center, plane_normal, ray_o_world, ray_p_world);
//			if(plane_intersection_point != ray_o_world)
//			{
//				//pixel = vec4(0, 1.0, 0, 1.0);
//				//pixel = vec4(0.0, 0.8, 0.0, 1.0);
//				vec3 direction = normalize(reflect(ray_p_world, plane_normal));
//				pixel = texture(cubeMap, vec3(direction.x, -direction.y, direction.z));
//				pixel = mix(pixel, vec4(0.0, 0.0, 0.0, 1.0), 0.75);
//			}
//			else
//			{
//				pixel = vec4(0, 0.0, 0.8, 1.0);
//			}
//			
//		} 
//	}
//	else if(sphere2_intersection_point != ray_o_world)
//	{
//		vec3 sphere_surface_normal = normalize(sphere2_intersection_point - sphere2.xyz);
//		ray_p_world = reflect(ray_p_world, sphere_surface_normal);
//
//		vec3 plane_intersection_point = plane_point_intersect(plane_center, -plane_normal, sphere2_intersection_point, ray_p_world);
//		if(plane_intersection_point != sphere2_intersection_point)
//		{
//			pixel = texture(cubeMap, normalize(vec3(ray_p_world.x, -ray_p_world.y, ray_p_world.z)));
//			pixel = mix(pixel, vec4(0.0, 0.0, 0.0, 1.0), 0.25);	
//		}
//		else
//		{
//			vec3 plane_intersection_point = plane_point_intersect(plane_center, plane_normal, ray_o_world, ray_p_world);
//			if(plane_intersection_point != ray_o_world)
//			{
//				//pixel = vec4(0, 1.0, 0, 1.0);
//				//pixel = vec4(0.0, 0.8, 0.0, 1.0);
//				vec3 direction = normalize(reflect(ray_p_world, plane_normal));
//				pixel = texture(cubeMap, vec3(direction.x, -direction.y, direction.z));
//				pixel = mix(pixel, vec4(0.0, 0.0, 0.0, 1.0), 0.75);
//			}
//			else
//			{
//				pixel = vec4(0, 0.0, 0.8, 1.0);
//			}
//		} 
//	}
//	else
//	{
//		vec3 plane_intersection_point = plane_point_intersect(plane_center, plane_normal, ray_o_world, ray_p_world);
//		if(plane_intersection_point == ray_o_world)
//		{
//			//pixel = vec4(0, 1.0, 0, 1.0);
//			pixel = texture(cubeMap, normalize(vec3(ray_p_world.x, -ray_p_world.y, ray_p_world.z)));
//			imageStore(img_output, pixel_coords, pixel);
//			return;
//		}
//		vec3 to_light_vector = normalize(light_source - plane_intersection_point);
//
//		vec3 direction = normalize(reflect(ray_p_world, plane_normal));
//		pixel = texture(cubeMap, vec3(direction.x, -direction.y, direction.z));
//		pixel = mix(pixel, vec4(0.0, 0.0, 0.0, 1.0), 0.75);
//		//pixel = vec4(1.0, 0.0, 0.0, 1.0);
//
//		sphere_intersection_point = sphere_point_intersect(sphere, plane_intersection_point, to_light_vector);
//		sphere2_intersection_point = sphere_point_intersect(sphere2, plane_intersection_point, to_light_vector);
//		if(sphere_intersection_point != plane_intersection_point)
//		{
//			pixel = mix(pixel, vec4(0.0, 0.0, 0.0, 1.0), 0.3);
//		}
//		else if(sphere2_intersection_point != plane_intersection_point)
//		{
//			pixel = mix(pixel, vec4(0.0, 0.0, 0.0, 1.0), 0.3);
//		}
//	}
//
//	 //ALMOST FUNCTIONAL (END)


	// PLANE NORMAL INTERSECTION TEST
//	vec3 plane_intersect_point = plane_point_intersect(plane_center, plane_normal, ray_o_world, ray_p_world);
//	if(plane_intersect_point == ray_o_world)
//	{
//		pixel = vec4(0.0, 1.0, 0.0, 1.0);
//	}
//	else
//	{
//		pixel = vec4(1.0, 0.0, 0.0, 1.0);
//	}
	// PLANE NORMAL INTERSECTION TEST (END)
	
			//RT
			//	vec3 ray_origin = ray_o_world;
			//	vec3 ray_direction = ray_p_world;
			//	for(int i = 0; i < 10; i++)
			//	{
			//		vec3 sphere_intersect_point = sphere_point_intersect(sphere, ray_origin, ray_direction);
			//		if(sphere_intersect_point == ray_origin)
			//		{
			//			vec3 plane_intersect_point = plane_intersect(plane_center, plane_normal, ray_origin, ray_direction);
			//			if(plane_intersect_point == ray_origin)
			//			{
			//			    if(i == 1)
			//				{
			//					pixel += vec4(1.0, 0.0, 0.0, 0.0);
			//				}
			//				else
			//				{
			//					pixel += vec4(0.0, 1.0, 0.0, 0.0);
			//				}
			//				
			//				break;
			//			}
			//
			//			vec3 to_light_direction = normalize(light_source - plane_intersect_point);
			//
			//			ray_origin = plane_intersect_point;
			//			ray_direction = to_light_direction;
			//		}
			//		else
			//		{
			//			pixel += vec4(0.1, 0.1, 0.1, 1.0);
			//			//pixel = mix(pixel, vec4(0.1, 0.1, 0.1, 1.0), 0.9);
			//			vec3 sphere_surface_normal = normalize(sphere_intersect_point - sphere.xyz);
			//			ray_direction = reflect(ray_direction, sphere_surface_normal);
			//			ray_origin = sphere_intersect_point + ray_direction * 0.01f;
			//		}
			//	}
	

	//float t = plane_intersect(plane_center, plane_normal, ray_p_world, ray_o_world);
	//   vec3 plane_intersection = plane_intersect(plane_center, plane_normal, ray_p_world, ray_o_world);
	//pixel = vec4(abs(plane_intersection.x), abs(plane_intersection.x), abs(plane_intersection.x), 1.0);

	//   vec3 reflected_ray_direction = reflect(ray_p_world, plane_normal);

//	if(sphere_intersect(sphere, plane_intersection, reflected_ray_direction))
//	{
//		pixel = vec4(0.0, 0.0, 0.0, 1.0);
//	}

//	if(intersects(sphere, ray_o_world, ray_p_world))
//	{
//		pixel = vec4(1.0, 1.0, 1.0, 1.0);
//	}

	//if(x > 0.5) pixel = vec4(1.0, 1.0, 1.0, 1.0);

	imageStore(img_output, pixel_coords, pixel);
}