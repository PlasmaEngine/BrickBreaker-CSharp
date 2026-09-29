AssetProfiles
{
	Config %Default
	{
		Objects
		{
			o
			{
				Uuid %id{uint64{10386675348308922682,207938457348376428}}
				string %t{"plCoreRenderProfileConfig"}
				uint32 %v{1}
				p
				{
					uint32 %MaxShadowMapSize{1024}
					uint32 %MinShadowMapSize{64}
					uint32 %RuntimeDecalAtlasTextureSize{3072}
					uint32 %ShadowAtlasTextureSize{4096}
				}
			}
			o
			{
				Uuid %id{uint64{385040311378845408,1178138948935131612}}
				string %t{"plRenderPipelineProfileConfig"}
				uint32 %v{1}
				p
				{
					VarDict %CameraPipelines{}
					string %MainRenderPipeline{"{ 308de3b9-bda1-5371-a3ca-4ae3744141f5 }"}
				}
			}
			o
			{
				Uuid %id{uint64{6024007684197752254,9388485812360408817}}
				string %t{"plTextureAssetProfileConfig"}
				uint32 %v{1}
				p
				{
					uint16 %MaxResolution{16384}
				}
			}
			o
			{
				Uuid %id{uint64{3821783988184046669,15370839493438779908}}
				string %t{"plXRConfig"}
				uint32 %v{2}
				p
				{
					string %BlendMode{"plXREnvironmentBlendMode::Opaque"}
					bool %EnableXR{false}
					uint8 %MsaaSampleCount{1}
					string %XRRenderPipeline{"{ 2fe25ded-776c-7f9e-354f-e4c52a33d125 }"}
				}
			}
			o
			{
				Uuid %id{uint64{8341519292606584866,16089769571062246001}}
				string %t{"plPlatformProfile"}
				uint32 %v{1}
				string %n{"root"}
				p
				{
					VarArray %Configs
					{
						Uuid{uint64{10386675348308922682,207938457348376428}}
						Uuid{uint64{385040311378845408,1178138948935131612}}
						Uuid{uint64{6024007684197752254,9388485812360408817}}
						Uuid{uint64{3821783988184046669,15370839493438779908}}
					}
					string %Name{"Default"}
					string %TargetPlatform{"Windows"}
				}
			}
		}
	}
	Config %Linux
	{
		Objects
		{
			o
			{
				Uuid %id{uint64{15318696607724717460,6045110293106184905}}
				string %t{"plTextureAssetProfileConfig"}
				uint32 %v{1}
				p
				{
					uint16 %MaxResolution{16384}
				}
			}
			o
			{
				Uuid %id{uint64{5839525325585937867,9470424167665436894}}
				string %t{"plXRConfig"}
				uint32 %v{2}
				p
				{
					string %BlendMode{"plXREnvironmentBlendMode::Opaque"}
					bool %EnableXR{false}
					uint8 %MsaaSampleCount{1}
					string %XRRenderPipeline{"{ 2fe25ded-776c-7f9e-354f-e4c52a33d125 }"}
				}
			}
			o
			{
				Uuid %id{uint64{18117095289529290612,13330136855958037737}}
				string %t{"plCoreRenderProfileConfig"}
				uint32 %v{1}
				p
				{
					uint32 %MaxShadowMapSize{1024}
					uint32 %MinShadowMapSize{64}
					uint32 %RuntimeDecalAtlasTextureSize{3072}
					uint32 %ShadowAtlasTextureSize{4096}
				}
			}
			o
			{
				Uuid %id{uint64{10409216501488640469,17110137226627428592}}
				string %t{"plPlatformProfile"}
				uint32 %v{1}
				string %n{"root"}
				p
				{
					VarArray %Configs
					{
						Uuid{uint64{18117095289529290612,13330136855958037737}}
						Uuid{uint64{11850880575465069964,18068185164358754495}}
						Uuid{uint64{15318696607724717460,6045110293106184905}}
						Uuid{uint64{5839525325585937867,9470424167665436894}}
					}
					string %Name{"Linux"}
					string %TargetPlatform{"Linux"}
				}
			}
			o
			{
				Uuid %id{uint64{11850880575465069964,18068185164358754495}}
				string %t{"plRenderPipelineProfileConfig"}
				uint32 %v{1}
				p
				{
					VarDict %CameraPipelines{}
					string %MainRenderPipeline{"{ 308de3b9-bda1-5371-a3ca-4ae3744141f5 }"}
				}
			}
		}
	}
	Config %Android
	{
		Objects
		{
			o
			{
				Uuid %id{uint64{6254889518651497815,1925153709652274566}}
				string %t{"plCoreRenderProfileConfig"}
				uint32 %v{1}
				p
				{
					uint32 %MaxShadowMapSize{1024}
					uint32 %MinShadowMapSize{64}
					uint32 %RuntimeDecalAtlasTextureSize{3072}
					uint32 %ShadowAtlasTextureSize{4096}
				}
			}
			o
			{
				Uuid %id{uint64{288558262873517458,9699341865981989901}}
				string %t{"plXRConfig"}
				uint32 %v{2}
				p
				{
					string %BlendMode{"plXREnvironmentBlendMode::Opaque"}
					bool %EnableXR{false}
					uint8 %MsaaSampleCount{1}
					string %XRRenderPipeline{"{ 2fe25ded-776c-7f9e-354f-e4c52a33d125 }"}
				}
			}
			o
			{
				Uuid %id{uint64{923818555026311103,12972827684282822178}}
				string %t{"plRenderPipelineProfileConfig"}
				uint32 %v{1}
				p
				{
					VarDict %CameraPipelines{}
					string %MainRenderPipeline{"{ 308de3b9-bda1-5371-a3ca-4ae3744141f5 }"}
				}
			}
			o
			{
				Uuid %id{uint64{11592751623319371637,13121262766510098711}}
				string %t{"plPlatformProfile"}
				uint32 %v{1}
				string %n{"root"}
				p
				{
					VarArray %Configs
					{
						Uuid{uint64{6254889518651497815,1925153709652274566}}
						Uuid{uint64{923818555026311103,12972827684282822178}}
						Uuid{uint64{15844351413070786501,15792889753157021679}}
						Uuid{uint64{288558262873517458,9699341865981989901}}
					}
					string %Name{"Android"}
					string %TargetPlatform{"Android"}
				}
			}
			o
			{
				Uuid %id{uint64{15844351413070786501,15792889753157021679}}
				string %t{"plTextureAssetProfileConfig"}
				uint32 %v{1}
				p
				{
					uint16 %MaxResolution{16384}
				}
			}
		}
	}
}
