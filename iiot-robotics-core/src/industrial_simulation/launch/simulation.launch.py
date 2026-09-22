import os
from ament_index_python.packages import get_package_share_directory
from launch import LaunchDescription
from launch.actions import IncludeLaunchDescription
from launch.launch_description_sources import PythonLaunchDescriptionSource

def generate_launch_description():
    pkg_share = get_package_share_directory('industrial_simulation')
    world_file = os.path.join(pkg_share, 'worlds', 'factory.world')

    # Lançar o Gazebo simulador em modo headless (servidor)
    gazebo = IncludeLaunchDescription(
        PythonLaunchDescriptionSource([
            os.path.join(get_package_share_directory('ros_gz_sim'), 'launch', 'gz_sim.launch.py')
        ]),
        launch_arguments={'gz_args': f'-r -s {world_file}'}.items(),
    )

    return LaunchDescription([
        gazebo
    ])