#include <rclcpp/rclcpp.hpp>
#include <std_msgs/msg/string.hpp>
#include <modbus/modbus.h>
#include <thread>
#include <chrono>
#include <unistd.h>

class CellControllerNode : public rclcpp::Node
{
public:
    CellControllerNode() : Node("cell_controller_node")
    {
        state_publisher_ = this->create_publisher<std_msgs::msg::String>("cell_status", 10);
        timer_ = this->create_wall_timer(
            std::chrono::seconds(1),
            std::bind(&CellControllerNode::timer_callback, this)
        );

        // Inicia a thread do servidor Modbus TCP
        modbus_thread_ = std::thread(&CellControllerNode::run_modbus_server, this);
    }

    ~CellControllerNode()
    {
        if (modbus_thread_.joinable()) {
            modbus_thread_.join();
        }
    }

private:
    void timer_callback()
    {
        auto message = std_msgs::msg::String();
        message.data = "Cell Controller Running - Modbus TCP OK";
        state_publisher_->publish(message);
    }

    void run_modbus_server()
    {
        modbus_t *ctx = modbus_new_tcp("0.0.0.0", 5020);
        modbus_mapping_t *mb_mapping = modbus_mapping_new(50, 50, 50, 50);
        
        if (ctx == nullptr || mb_mapping == nullptr) {
            RCLCPP_ERROR(this->get_logger(), "Falha ao alocar contexto/mapping Modbus");
            return;
        }

        int server_socket = modbus_tcp_listen(ctx, 1);
        if (server_socket == -1) {
            RCLCPP_ERROR(this->get_logger(), "Erro no listen do Modbus TCP");
            modbus_free(ctx);
            modbus_mapping_free(mb_mapping);
            return;
        }

        RCLCPP_INFO(this->get_logger(), "Servidor Modbus TCP rodando na porta 5020.");

        while (rclcpp::ok()) {
            int accept_socket = modbus_tcp_accept(ctx, &server_socket);
            if (accept_socket != -1) {
                while (rclcpp::ok()) {
                    uint8_t query[MODBUS_TCP_MAX_ADU_LENGTH];
                    int rc = modbus_receive(ctx, query);
                    if (rc == -1) {
                        break;
                    }
                    modbus_reply(ctx, query, rc, mb_mapping);
                }
                close(accept_socket);
            }
        }

        close(server_socket);
        modbus_free(ctx);
        modbus_mapping_free(mb_mapping);
    }

    rclcpp::Publisher<std_msgs::msg::String>::SharedPtr state_publisher_;
    rclcpp::TimerBase::SharedPtr timer_;
    std::thread modbus_thread_;
};

int main(int argc, char * argv[])
{
    rclcpp::init(argc, argv);
    auto node = std::make_shared<CellControllerNode>();
    rclcpp::spin(node);
    rclcpp::shutdown();
    return 0;
}